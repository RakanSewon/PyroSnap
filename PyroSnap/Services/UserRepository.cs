using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Npgsql;

namespace PyroSnap.Services
{
    /// <summary>Akses tabel <c>users</c> (AUTH-01 / DBA-06). Query selalu berparameter.</summary>
    public static class UserRepository
    {
        public const int PasswordMin = 8;
        public const int PasswordMaks = 72;   // batas input BCrypt (byte)
        private static readonly Regex PolaUsername = new("^[A-Za-z0-9_.]{3,30}$");

        /// <summary>
        /// Validasi data daftar mandiri; null bila valid, selain itu pesan untuk pengguna.
        /// </summary>
        public static string? ValidasiDaftar(string username, string password, string konfirmasi)
        {
            if (!PolaUsername.IsMatch(username))
                return "Username 3–30 karakter, hanya huruf, angka, titik, atau garis bawah.";
            if (password.Length < PasswordMin)
                return $"Password minimal {PasswordMin} karakter.";
            if (Encoding.UTF8.GetByteCount(password) > PasswordMaks)
                return "Password terlalu panjang.";
            if (password != konfirmasi)
                return "Konfirmasi password tidak sama.";
            return null;
        }

        /// <summary>
        /// Daftar mandiri warga (ROLE-04, K10): akun baru selalu berperan Publik.
        /// Melempar <see cref="ArgumentException"/> bila data tidak valid dan
        /// <see cref="InvalidOperationException"/> bila username sudah dipakai atau database
        /// belum dimigrasi (008).
        /// </summary>
        public static async Task<User> DaftarPublikAsync(string username, string password, string konfirmasi)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            return await DaftarPublikAsync(conn, username, password, konfirmasi);
        }

        internal static async Task<User> DaftarPublikAsync(NpgsqlConnection conn,
            string username, string password, string konfirmasi)
        {
            username = username.Trim();
            string? salah = ValidasiDaftar(username, password, konfirmasi);
            if (salah != null) throw new ArgumentException(salah);

            // BCrypt sengaja lambat; jangan di thread UI.
            string hash = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(password, 11));

            await using var cmd = new NpgsqlCommand(
                "INSERT INTO users (username, password_hash, role) VALUES (@username, @hash, 'Publik') " +
                "RETURNING user_id", conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("hash", hash);

            try
            {
                int id = (int)(await cmd.ExecuteScalarAsync())!;
                return new User
                {
                    UserId = id.ToString(CultureInfo.InvariantCulture),
                    Username = username,
                    PasswordHash = hash,
                    Role = "Publik"
                };
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new InvalidOperationException("Username sudah dipakai. Pilih username lain.");
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.CheckViolation)
            {
                throw new InvalidOperationException(
                    "Database belum mendukung akun Publik. Jalankan migrasi database/008_peran_publik.sql.");
            }
        }

        /// <summary>Cari akun berdasarkan username; null bila tidak ada.</summary>
        public static async Task<User?> FindByUsernameAsync(string username)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT user_id, username, password_hash, role FROM users WHERE username = @username",
                conn);
            cmd.Parameters.AddWithValue("username", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return new User
            {
                UserId = reader.GetInt32(0).ToString(CultureInfo.InvariantCulture),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Role = reader.GetString(3)
            };
        }
    }
}
