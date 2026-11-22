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
        private const string PesanBelumMigrasi =
            "Database belum diperbarui. Jalankan migrasi database/010_fungsi_akun.sql.";

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
        /// belum dimigrasi (010).
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

            // Hash dibuat di database; user aplikasi tidak punya akses ke kolom password_hash.
            await using var cmd = new NpgsqlCommand("SELECT daftar_publik(@username, @password)", conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("password", password);

            try
            {
                int id = (int)(await cmd.ExecuteScalarAsync())!;
                var user = new User
                {
                    UserId = id.ToString(CultureInfo.InvariantCulture),
                    Username = username,
                    Role = "Publik"
                };
                user.TandaiLoginTerverifikasi();
                return user;
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new InvalidOperationException("Username sudah dipakai. Pilih username lain.");
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.InvalidParameterValue)
            {
                throw new ArgumentException(e.MessageText);
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UndefinedFunction)
            {
                throw new InvalidOperationException(PesanBelumMigrasi);
            }
        }

        /// <summary>
        /// Verifikasi login di database (fungsi masuk_akun). Null bila username atau password salah;
        /// keduanya sengaja tidak dibedakan (ACC-03/04).
        /// </summary>
        public static async Task<User?> MasukAsync(string username, string password)
        {
            await using var conn = await DatabaseConnection.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT user_id, username, role FROM masuk_akun(@username, @password)", conn);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("password", password);

            try
            {
                await using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return null;

                var user = new User
                {
                    UserId = reader.GetInt32(0).ToString(CultureInfo.InvariantCulture),
                    Username = reader.GetString(1),
                    Role = reader.GetString(2)
                };
                user.TandaiLoginTerverifikasi();
                return user;
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UndefinedFunction)
            {
                throw new InvalidOperationException(PesanBelumMigrasi);
            }
        }
    }
}
