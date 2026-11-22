using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace PyroSnap.Services
{
    /// <summary>
    /// Akses tabel <c>hotspot</c> dan <c>hotspot_data_source</c> (DBA-04, DBA-05, BE-11).
    /// Semua query berparameter. Waktu disimpan sebagai UTC (TIMESTAMPTZ).
    /// Overload <c>internal</c> menerima koneksi yang sudah terbuka (dipakai bersama
    /// ReportRepository dalam satu transaksi, dan untuk pengujian).
    /// </summary>
    public static class HotspotRepository
    {
        /// <summary>Data yang ditampilkan dari database: 24 jam sebelum akuisisi terbaru.</summary>
        public static readonly TimeSpan JendelaTampil = TimeSpan.FromHours(24);

        /// <summary>
        /// Simpan hasil sinkronisasi: hotspot baru di-insert (duplikat diabaikan) dan
        /// waktu sinkronisasi sumber diperbarui, dalam satu transaksi.
        /// </summary>
        /// <returns>Jumlah hotspot yang benar-benar baru tersimpan.</returns>
        public static async Task<int> SimpanHasilSinkronAsync(
            string namaSumber, string endpointTanpaKey, IReadOnlyList<Hotspot> hotspots, DateTime waktuSinkron)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await SimpanHasilSinkronAsync(conn, namaSumber, endpointTanpaKey, hotspots, waktuSinkron);
        }

        internal static async Task<int> SimpanHasilSinkronAsync(NpgsqlConnection conn,
            string namaSumber, string endpointTanpaKey, IReadOnlyList<Hotspot> hotspots, DateTime waktuSinkron)
        {
            await using var tx = await conn.BeginTransactionAsync();

            int sourceId = await SourceIdAsync(conn, tx, namaSumber, endpointTanpaKey);
            int baru = await UpsertAsync(conn, tx, sourceId, hotspots);

            await using (var cmd = new NpgsqlCommand(
                "UPDATE hotspot_data_source SET waktu_sinkronisasi = @waktu WHERE source_id = @source", conn, tx))
            {
                cmd.Parameters.AddWithValue("waktu", KeUtc(waktuSinkron));
                cmd.Parameters.AddWithValue("source", sourceId);
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return baru;
        }

        /// <summary>
        /// Muat hotspot terbaru sebuah sumber (jendela <see cref="JendelaTampil"/> sebelum
        /// akuisisi terakhir) beserta waktu sinkronisasi terakhirnya (UTC).
        /// </summary>
        public static async Task<(List<Hotspot> Data, DateTime? WaktuSinkron)> MuatTerbaruAsync(string namaSumber)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await MuatTerbaruAsync(conn, namaSumber);
        }

        internal static async Task<(List<Hotspot> Data, DateTime? WaktuSinkron)> MuatTerbaruAsync(
            NpgsqlConnection conn, string namaSumber)
        {
            DateTime? waktuSinkron = null;
            DateTime? akuisisiTerbaru = null;

            await using (var cmd = new NpgsqlCommand(
                "SELECT s.waktu_sinkronisasi, " +
                "       (SELECT max(h.waktu_akuisisi) FROM hotspot h WHERE h.source_id = s.source_id) " +
                "FROM hotspot_data_source s WHERE s.nama_sumber = @nama", conn))
            {
                cmd.Parameters.AddWithValue("nama", namaSumber);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(0)) waktuSinkron = reader.GetDateTime(0);
                    if (!reader.IsDBNull(1)) akuisisiTerbaru = reader.GetDateTime(1);
                }
            }

            if (akuisisiTerbaru == null) return (new List<Hotspot>(), waktuSinkron);

            var filter = new FilterCriteria { WaktuMulai = akuisisiTerbaru.Value - JendelaTampil };
            return (await QueryAsync(conn, namaSumber, filter), waktuSinkron);
        }

        /// <summary>
        /// Hotspot sebuah sumber yang memenuhi <paramref name="filter"/>; hasilnya sama dengan
        /// <see cref="FilterCriteria.IsMatch"/> (waktu inklusif, confidence &amp; FRP minimum).
        /// </summary>
        public static async Task<List<Hotspot>> QueryAsync(string namaSumber, FilterCriteria filter)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await QueryAsync(conn, namaSumber, filter);
        }

        internal static async Task<List<Hotspot>> QueryAsync(
            NpgsqlConnection conn, string namaSumber, FilterCriteria filter)
        {
            await using var cmd = new NpgsqlCommand { Connection = conn };
            var sql = new StringBuilder(
                "SELECT h.latitude::float8, h.longitude::float8, h.confidence, " +
                "       COALESCE(h.brightness, 0)::float8, COALESCE(h.frp, 0)::float8, h.waktu_akuisisi " +
                "FROM hotspot h JOIN hotspot_data_source s ON s.source_id = h.source_id " +
                "WHERE s.nama_sumber = @nama AND h.confidence >= @confMin AND COALESCE(h.frp, 0) >= @frpMin");

            cmd.Parameters.AddWithValue("nama", namaSumber);
            cmd.Parameters.AddWithValue("confMin", filter.ConfidenceMin);
            cmd.Parameters.AddWithValue("frpMin", filter.FrpMin);

            // MinValue/MaxValue = tanpa batas (nilai bawaan FilterCriteria.Reset()).
            if (filter.WaktuMulai != DateTime.MinValue)
            {
                sql.Append(" AND h.waktu_akuisisi >= @mulai");
                cmd.Parameters.AddWithValue("mulai", KeUtc(filter.WaktuMulai));
            }
            if (filter.WaktuAkhir != DateTime.MaxValue)
            {
                sql.Append(" AND h.waktu_akuisisi <= @akhir");
                cmd.Parameters.AddWithValue("akhir", KeUtc(filter.WaktuAkhir));
            }
            sql.Append(" ORDER BY h.waktu_akuisisi, h.hotspot_id");
            cmd.CommandText = sql.ToString();

            var hasil = new List<Hotspot>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                hasil.Add(new Hotspot(
                    reader.GetDouble(0), reader.GetDouble(1), reader.GetInt16(2),
                    reader.GetDouble(3), reader.GetDouble(4), reader.GetDateTime(5)));
            }
            return hasil;
        }

        /// <summary>Id sumber berdasarkan nama; dibuat bila belum ada (butuh UNIQUE nama_sumber).</summary>
        internal static async Task<int> SourceIdAsync(
            NpgsqlConnection conn, NpgsqlTransaction? tx, string namaSumber, string endpointTanpaKey)
        {
            await using (var insert = new NpgsqlCommand(
                "INSERT INTO hotspot_data_source (nama_sumber, endpoint_url) VALUES (@nama, @url) " +
                "ON CONFLICT (nama_sumber) DO NOTHING", conn, tx))
            {
                insert.Parameters.AddWithValue("nama", namaSumber);
                insert.Parameters.AddWithValue("url", endpointTanpaKey);
                await insert.ExecuteNonQueryAsync();
            }

            await using var select = new NpgsqlCommand(
                "SELECT source_id FROM hotspot_data_source WHERE nama_sumber = @nama", conn, tx);
            select.Parameters.AddWithValue("nama", namaSumber);
            return (int)(await select.ExecuteScalarAsync())!;
        }

        /// <summary>
        /// Insert banyak hotspot sekaligus; baris yang sudah ada (kunci unik sumber, posisi,
        /// waktu — SCH-01) diabaikan. Mengembalikan jumlah baris baru.
        /// </summary>
        internal static async Task<int> UpsertAsync(
            NpgsqlConnection conn, NpgsqlTransaction? tx, int sourceId, IReadOnlyList<Hotspot> hotspots)
        {
            if (hotspots.Count == 0) return 0;

            await using var cmd = new NpgsqlCommand(
                "INSERT INTO hotspot (latitude, longitude, confidence, brightness, frp, waktu_akuisisi, source_id) " +
                "SELECT t.lat, t.lon, t.conf, t.bright, t.frp, t.waktu, @source " +
                "FROM unnest(@lat, @lon, @conf, @bright, @frp, @waktu) AS t(lat, lon, conf, bright, frp, waktu) " +
                "ON CONFLICT ON CONSTRAINT uq_hotspot_source_pos_time DO NOTHING", conn, tx);

            cmd.Parameters.AddWithValue("source", sourceId);
            cmd.Parameters.AddWithValue("lat", hotspots.Select(h => h.Latitude).ToArray());
            cmd.Parameters.AddWithValue("lon", hotspots.Select(h => h.Longitude).ToArray());
            cmd.Parameters.AddWithValue("conf", hotspots.Select(h => h.Confidence).ToArray());
            cmd.Parameters.AddWithValue("bright", hotspots.Select(h => h.Brightness).ToArray());
            cmd.Parameters.AddWithValue("frp", hotspots.Select(h => h.Frp).ToArray());
            cmd.Parameters.AddWithValue("waktu", hotspots.Select(h => KeUtc(h.WaktuAkuisisi)).ToArray());

            return await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>Npgsql hanya menerima DateTime UTC untuk TIMESTAMPTZ.</summary>
        internal static DateTime KeUtc(DateTime waktu) => waktu.Kind switch
        {
            DateTimeKind.Utc => waktu,
            DateTimeKind.Local => waktu.ToUniversalTime(),
            _ => DateTime.SpecifyKind(waktu, DateTimeKind.Utc) // data FIRMS tanpa Kind = UTC
        };
    }
}
