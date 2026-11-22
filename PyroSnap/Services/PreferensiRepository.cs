using System.Threading.Tasks;
using Npgsql;

namespace PyroSnap.Services
{
    /// <summary>
    /// Wilayah pilihan warga (PUB-01) disimpan di tabel <c>filter_criteria</c> yang sudah ada
    /// (kolom <c>wilayah</c>, satu baris per user). Tamu tidak memakai ini (hanya sesi).
    /// </summary>
    public static class PreferensiRepository
    {
        public static async Task<string?> AmbilWilayahPilihanAsync(int userId)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await AmbilWilayahPilihanAsync(conn, userId);
        }

        internal static async Task<string?> AmbilWilayahPilihanAsync(NpgsqlConnection conn, int userId)
        {
            await using var cmd = new NpgsqlCommand(
                "SELECT wilayah FROM filter_criteria WHERE user_id = @user AND wilayah IS NOT NULL " +
                "ORDER BY filter_id DESC LIMIT 1", conn);
            cmd.Parameters.AddWithValue("user", userId);
            return await cmd.ExecuteScalarAsync() as string;
        }

        /// <summary>Simpan (atau hapus bila null) wilayah pilihan user dalam satu transaksi.</summary>
        public static async Task SimpanWilayahPilihanAsync(int userId, string? wilayah)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            await SimpanWilayahPilihanAsync(conn, userId, wilayah);
        }

        internal static async Task SimpanWilayahPilihanAsync(NpgsqlConnection conn, int userId, string? wilayah)
        {
            await using var tx = await conn.BeginTransactionAsync();

            await using (var hapus = new NpgsqlCommand(
                "DELETE FROM filter_criteria WHERE user_id = @user AND wilayah IS NOT NULL", conn, tx))
            {
                hapus.Parameters.AddWithValue("user", userId);
                await hapus.ExecuteNonQueryAsync();
            }

            if (!string.IsNullOrWhiteSpace(wilayah))
            {
                await using var simpan = new NpgsqlCommand(
                    "INSERT INTO filter_criteria (user_id, wilayah) VALUES (@user, @wilayah)", conn, tx);
                simpan.Parameters.AddWithValue("user", userId);
                simpan.Parameters.AddWithValue("wilayah", wilayah.Trim());
                await simpan.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
        }
    }
}
