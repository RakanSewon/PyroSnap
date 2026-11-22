using System;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace PyroSnap.Services
{
    /// <summary>
    /// Pencatatan laporan ke <c>hotspot_report</c> + <c>report_hotspot</c> (DBA-07, RPT-04, RPT-05).
    /// Semua langkah dalam satu transaksi: gagal di tengah = tidak ada data setengah jadi.
    /// </summary>
    public static class ReportRepository
    {
        private const int PanjangMaksFilePath = 255; // sesuai VARCHAR(255) di skema

        /// <summary>
        /// Catat laporan yang sudah di-<see cref="HotspotReport.Generate"/>.
        /// Hotspot di laporan ikut di-upsert dulu supaya selalu bisa ditautkan
        /// (relasi agregasi: hotspot tetap ada walau laporan dihapus).
        /// </summary>
        /// <returns>Id laporan baru dan jumlah hotspot yang ditautkan.</returns>
        public static async Task<(int ReportId, int JumlahHotspot)> SimpanAsync(
            HotspotReport report, int userId, string namaSumber, string endpointTanpaKey, string? filePath)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await SimpanAsync(conn, report, userId, namaSumber, endpointTanpaKey, filePath);
        }

        internal static async Task<(int ReportId, int JumlahHotspot)> SimpanAsync(NpgsqlConnection conn,
            HotspotReport report, int userId, string namaSumber, string endpointTanpaKey, string? filePath)
        {
            if (report.DaftarHotspot.Count == 0)
                throw new InvalidOperationException("Laporan tanpa hotspot tidak dicatat.");

            await using var tx = await conn.BeginTransactionAsync();

            int sourceId = await HotspotRepository.SourceIdAsync(conn, tx, namaSumber, endpointTanpaKey);
            await HotspotRepository.UpsertAsync(conn, tx, sourceId, report.DaftarHotspot);

            int reportId;
            await using (var cmd = new NpgsqlCommand(
                "INSERT INTO hotspot_report (user_id, format, tanggal_dibuat, file_path) " +
                "VALUES (@user, @format, @tanggal, @path) RETURNING report_id", conn, tx))
            {
                DateTime tanggal = report.TanggalDibuat == default ? DateTime.UtcNow : report.TanggalDibuat;
                bool pathMuat = filePath != null && filePath.Length <= PanjangMaksFilePath;

                cmd.Parameters.AddWithValue("user", userId);
                cmd.Parameters.AddWithValue("format", report.Format.ToUpperInvariant());
                cmd.Parameters.AddWithValue("tanggal", HotspotRepository.KeUtc(tanggal));
                cmd.Parameters.AddWithValue("path", pathMuat ? filePath! : DBNull.Value);
                reportId = (int)(await cmd.ExecuteScalarAsync())!;
            }

            // Tautkan lewat kunci alami (sumber, posisi dibulatkan seperti kolom NUMERIC(9,6), waktu).
            int jumlah;
            await using (var cmd = new NpgsqlCommand(
                "INSERT INTO report_hotspot (report_id, hotspot_id) " +
                "SELECT DISTINCT @report, h.hotspot_id " +
                "FROM unnest(@lat, @lon, @waktu) AS t(lat, lon, waktu) " +
                "JOIN hotspot h ON h.source_id = @source " +
                "              AND h.latitude = round(t.lat::numeric, 6) " +
                "              AND h.longitude = round(t.lon::numeric, 6) " +
                "              AND h.waktu_akuisisi = t.waktu " +
                "ON CONFLICT DO NOTHING", conn, tx))
            {
                var data = report.DaftarHotspot;
                cmd.Parameters.AddWithValue("report", reportId);
                cmd.Parameters.AddWithValue("source", sourceId);
                cmd.Parameters.AddWithValue("lat", data.Select(h => h.Latitude).ToArray());
                cmd.Parameters.AddWithValue("lon", data.Select(h => h.Longitude).ToArray());
                cmd.Parameters.AddWithValue("waktu", data.Select(h => HotspotRepository.KeUtc(h.WaktuAkuisisi)).ToArray());
                jumlah = await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return (reportId, jumlah);
        }
    }
}
