using System;
using System.Threading.Tasks;
using Npgsql;

namespace PyroSnap.Services
{
        public static class DatabaseConnection
    {
        public static string ConnectionString =>
            Environment.GetEnvironmentVariable("PYROSNAP_DB")
            ?? "Host=localhost;Port=5432;Database=pyrosnap;Username=postgres";

        public static async Task<NpgsqlConnection> OpenAsync()
        {
            var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            return conn;
        }

        public static async Task<string> TestConnectionAsync()
        {
            await using var conn = await OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT version(), current_database(), " +
                "(SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public')",
                conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            return $"Terhubung ke database '{reader.GetString(1)}' "
                 + $"({reader.GetInt64(2)} tabel di schema public)\n{reader.GetString(0)}";
        }
    }
}
