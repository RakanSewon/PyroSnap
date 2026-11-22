using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace PyroSnap.Services
{
    /// <summary>
    /// Akses tabel <c>hotspot_verifikasi</c> (OPS-01, migrasi 009). Hotspot dicari lewat
    /// kunci alami (sumber, posisi, waktu) karena class Hotspot tidak menyimpan id.
    /// </summary>
    public static class VerifikasiRepository
    {
        public const int PanjangMaksCatatan = 500;

        private const string JoinHotspot =
            "FROM hotspot h JOIN hotspot_data_source s ON s.source_id = h.source_id " +
            "WHERE s.nama_sumber = @nama " +
            "  AND h.latitude = round(@lat::numeric, 6) AND h.longitude = round(@lon::numeric, 6) " +
            "  AND h.waktu_akuisisi = @waktu";

        /// <summary>
        /// Simpan hasil cek lapangan. Mengembalikan false bila hotspot belum ada di database.
        /// </summary>
        public static async Task<bool> SimpanAsync(
            string namaSumber, Hotspot hotspot, int userId, string status, string? catatan)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await SimpanAsync(conn, namaSumber, hotspot, userId, status, catatan);
        }

        internal static async Task<bool> SimpanAsync(NpgsqlConnection conn,
            string namaSumber, Hotspot hotspot, int userId, string status, string? catatan)
        {
            if (!Verifikasi.SemuaStatus.Contains(status))
                throw new ArgumentException($"Status verifikasi tidak dikenal: {status}");

            string? isiCatatan = string.IsNullOrWhiteSpace(catatan) ? null : catatan.Trim();
            if (isiCatatan != null && isiCatatan.Length > PanjangMaksCatatan)
                throw new ArgumentException($"Catatan maksimal {PanjangMaksCatatan} karakter.");

            await using var cmd = new NpgsqlCommand(
                "INSERT INTO hotspot_verifikasi (hotspot_id, user_id, status, catatan) " +
                "SELECT h.hotspot_id, @user, @status, @catatan " + JoinHotspot + " LIMIT 1", conn);
            IsiKunci(cmd, namaSumber, hotspot);
            cmd.Parameters.AddWithValue("user", userId);
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("catatan", (object?)isiCatatan ?? DBNull.Value);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        /// <summary>Riwayat verifikasi hotspot, terbaru lebih dulu.</summary>
        public static async Task<List<Verifikasi>> RiwayatAsync(string namaSumber, Hotspot hotspot, int batas = 5)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await RiwayatAsync(conn, namaSumber, hotspot, batas);
        }

        internal static async Task<List<Verifikasi>> RiwayatAsync(NpgsqlConnection conn,
            string namaSumber, Hotspot hotspot, int batas = 5)
        {
            await using var cmd = new NpgsqlCommand(
                "SELECT v.status, v.catatan, v.waktu, u.username " +
                "FROM hotspot_verifikasi v LEFT JOIN users u ON u.user_id = v.user_id " +
                "WHERE v.hotspot_id IN (SELECT h.hotspot_id " + JoinHotspot + ") " +
                "ORDER BY v.waktu DESC, v.verifikasi_id DESC LIMIT @batas", conn);
            IsiKunci(cmd, namaSumber, hotspot);
            cmd.Parameters.AddWithValue("batas", batas);

            var hasil = new List<Verifikasi>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                hasil.Add(new Verifikasi
                {
                    Status = reader.GetString(0),
                    Catatan = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Waktu = reader.GetDateTime(2),
                    Pemeriksa = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }
            return hasil;
        }

        private static void IsiKunci(NpgsqlCommand cmd, string namaSumber, Hotspot hotspot)
        {
            cmd.Parameters.AddWithValue("nama", namaSumber);
            cmd.Parameters.AddWithValue("lat", hotspot.Latitude);
            cmd.Parameters.AddWithValue("lon", hotspot.Longitude);
            cmd.Parameters.AddWithValue("waktu", HotspotRepository.KeUtc(hotspot.WaktuAkuisisi));
        }
    }
}
