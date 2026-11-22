using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace PyroSnap.Services
{
    /// <summary>
    /// Akses tabel <c>alert</c> (DBA-08, ALR-05). Alert dikirim ke semua user
    /// (keputusan 2026-11-22). Kunci unik (hotspot, penerima) dari migrasi 006
    /// mencegah alert ganda saat sinkronisasi ulang.
    /// </summary>
    public static class AlertRepository
    {
        /// <summary>
        /// Simpan alert untuk setiap user. Hotspot harus sudah tersimpan
        /// (HotspotRepository.SimpanHasilSinkronAsync). Mengembalikan jumlah baris alert baru.
        /// </summary>
        public static async Task<int> KirimKeSemuaUserAsync(
            string namaSumber, IReadOnlyList<(Hotspot Hotspot, Alert Alert)> daftar)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await KirimKeSemuaUserAsync(conn, namaSumber, daftar);
        }

        internal static async Task<int> KirimKeSemuaUserAsync(NpgsqlConnection conn,
            string namaSumber, IReadOnlyList<(Hotspot Hotspot, Alert Alert)> daftar)
        {
            if (daftar.Count == 0) return 0;

            await using var cmd = new NpgsqlCommand(
                "INSERT INTO alert (hotspot_id, target_user_id, message, \"timestamp\", is_read) " +
                "SELECT h.hotspot_id, u.user_id, t.pesan, t.waktu_kirim, false " +
                "FROM unnest(@lat, @lon, @waktu, @pesan, @kirim) AS t(lat, lon, waktu, pesan, waktu_kirim) " +
                "JOIN hotspot_data_source s ON s.nama_sumber = @nama " +
                "JOIN hotspot h ON h.source_id = s.source_id " +
                "              AND h.latitude = round(t.lat::numeric, 6) " +
                "              AND h.longitude = round(t.lon::numeric, 6) " +
                "              AND h.waktu_akuisisi = t.waktu " +
                "CROSS JOIN users u " +
                // ROLE-03 (K9): alert operasional hanya untuk Admin & Operator, bukan Publik.
                // 'Analis' disertakan untuk database yang belum menjalankan migrasi 008.
                "WHERE u.role IN ('Admin', 'Analis', 'Operator') " +
                "ON CONFLICT ON CONSTRAINT uq_alert_hotspot_user DO NOTHING", conn);

            cmd.Parameters.AddWithValue("nama", namaSumber);
            cmd.Parameters.AddWithValue("lat", daftar.Select(d => d.Hotspot.Latitude).ToArray());
            cmd.Parameters.AddWithValue("lon", daftar.Select(d => d.Hotspot.Longitude).ToArray());
            cmd.Parameters.AddWithValue("waktu", daftar.Select(d => HotspotRepository.KeUtc(d.Hotspot.WaktuAkuisisi)).ToArray());
            cmd.Parameters.AddWithValue("pesan", daftar.Select(d => d.Alert.Message).ToArray());
            cmd.Parameters.AddWithValue("kirim", daftar.Select(d => HotspotRepository.KeUtc(d.Alert.Timestamp)).ToArray());

            return await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>Alert milik user: belum dibaca dulu, lalu terbaru.</summary>
        public static async Task<List<Alert>> DaftarAsync(int userId, int batas = 100)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await DaftarAsync(conn, userId, batas);
        }

        internal static async Task<List<Alert>> DaftarAsync(NpgsqlConnection conn, int userId, int batas = 100) =>
            (await DaftarDenganHotspotAsync(conn, userId, batas)).Select(d => d.Alert).ToList();

        /// <summary>Alert milik user beserta hotspot pemicunya (UX-11: buka hotspot dari alert).</summary>
        public static async Task<List<(Alert Alert, Hotspot? Hotspot)>> DaftarDenganHotspotAsync(int userId, int batas = 100)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await DaftarDenganHotspotAsync(conn, userId, batas);
        }

        internal static async Task<List<(Alert Alert, Hotspot? Hotspot)>> DaftarDenganHotspotAsync(
            NpgsqlConnection conn, int userId, int batas = 100)
        {
            await using var cmd = new NpgsqlCommand(
                "SELECT a.alert_id, a.message, a.\"timestamp\", a.is_read, " +
                "       h.latitude::float8, h.longitude::float8, h.confidence, " +
                "       COALESCE(h.brightness, 0)::float8, COALESCE(h.frp, 0)::float8, h.waktu_akuisisi " +
                "FROM alert a LEFT JOIN hotspot h ON h.hotspot_id = a.hotspot_id " +
                "WHERE a.target_user_id = @user " +
                "ORDER BY a.is_read, a.\"timestamp\" DESC, a.alert_id DESC LIMIT @batas", conn);
            cmd.Parameters.AddWithValue("user", userId);
            cmd.Parameters.AddWithValue("batas", batas);

            var hasil = new List<(Alert, Hotspot?)>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var alert = new Alert
                {
                    AlertId = reader.GetInt32(0).ToString(CultureInfo.InvariantCulture),
                    Message = reader.GetString(1),
                    Timestamp = reader.GetDateTime(2),
                    IsRead = reader.GetBoolean(3)
                };
                Hotspot? hotspot = reader.IsDBNull(4) ? null : new Hotspot(
                    reader.GetDouble(4), reader.GetDouble(5), reader.GetInt16(6),
                    reader.GetDouble(7), reader.GetDouble(8), reader.GetDateTime(9));
                hasil.Add((alert, hotspot));
            }
            return hasil;
        }

        public static async Task<int> JumlahBelumDibacaAsync(int userId)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await JumlahBelumDibacaAsync(conn, userId);
        }

        internal static async Task<int> JumlahBelumDibacaAsync(NpgsqlConnection conn, int userId)
        {
            await using var cmd = new NpgsqlCommand(
                "SELECT count(*) FROM alert WHERE target_user_id = @user AND NOT is_read", conn);
            cmd.Parameters.AddWithValue("user", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        /// <summary>Tandai satu alert dibaca; hanya berlaku untuk alert milik user itu sendiri.</summary>
        public static async Task<bool> TandaiDibacaAsync(Alert alert, int userId)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await TandaiDibacaAsync(conn, alert, userId);
        }

        internal static async Task<bool> TandaiDibacaAsync(NpgsqlConnection conn, Alert alert, int userId)
        {
            if (!int.TryParse(alert.AlertId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int alertId))
                return false;

            await using var cmd = new NpgsqlCommand(
                "UPDATE alert SET is_read = true WHERE alert_id = @alert AND target_user_id = @user", conn);
            cmd.Parameters.AddWithValue("alert", alertId);
            cmd.Parameters.AddWithValue("user", userId);

            bool berhasil = await cmd.ExecuteNonQueryAsync() == 1;
            if (berhasil) alert.MarkAsRead();
            return berhasil;
        }

        /// <summary>Tandai semua alert milik user sebagai dibaca; mengembalikan jumlah yang berubah.</summary>
        public static async Task<int> TandaiSemuaDibacaAsync(int userId)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await TandaiSemuaDibacaAsync(conn, userId);
        }

        internal static async Task<int> TandaiSemuaDibacaAsync(NpgsqlConnection conn, int userId)
        {
            await using var cmd = new NpgsqlCommand(
                "UPDATE alert SET is_read = true WHERE target_user_id = @user AND NOT is_read", conn);
            cmd.Parameters.AddWithValue("user", userId);
            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
