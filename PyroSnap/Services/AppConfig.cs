using System;
using System.Collections.Generic;
using System.IO;

namespace PyroSnap.Services
{
    /// <summary>
    /// Konfigurasi aplikasi. Nilai dibaca dari environment variable lebih dulu,
    /// lalu dari file <c>.env</c> lokal (di-gitignore).
    /// Nilai rahasia (API key, password database) tidak boleh ditulis di kode.
    /// </summary>
    public static class AppConfig
    {
        public const string EnvDatabase = "PYROSNAP_DB";
        public const string EnvFirmsKey = "FIRMS_MAP_KEY";
        public const string EnvCartoKey = "CARTO_KEY";
        public const string EnvFirmsSource = "FIRMS_SOURCE";

        // VIIRS_SNPP_NRT berhenti mengirim data sejak 2026-11-02 (cek data_availability FIRMS);
        // NOAA-20 memakai format kolom yang sama.
        public const string DefaultFirmsSource = "VIIRS_NOAA20_NRT";

        // Batas folder induk yang diperiksa saat mencari .env
        // (bin/Debug/net8.0-windows → PyroSnap → root repo).
        private const int MaxParentLevels = 5;

        private static readonly Lazy<Dictionary<string, string>> _dotEnv =
            new(() => LoadDotEnv(FindDotEnv()));

        public static string? DatabaseConnectionString => Get(EnvDatabase);
        public static string? FirmsMapKey => Get(EnvFirmsKey);
        public static string? CartoKey => Get(EnvCartoKey);
        public static string FirmsSource => Get(EnvFirmsSource) ?? DefaultFirmsSource;

        /// <summary>Ambil nilai konfigurasi; null bila tidak diatur atau kosong.</summary>
        public static string? Get(string name)
        {
            string? fromEnv = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv.Trim();

            return _dotEnv.Value.TryGetValue(name, out string? fromFile)
                   && !string.IsNullOrWhiteSpace(fromFile)
                ? fromFile
                : null;
        }

        /// <summary>Cari .env mulai dari folder aplikasi, naik beberapa tingkat.</summary>
        private static string? FindDotEnv()
        {
            DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; dir != null && i <= MaxParentLevels; i++, dir = dir.Parent)
            {
                string path = Path.Combine(dir.FullName, ".env");
                if (File.Exists(path)) return path;
            }
            return null;
        }

        private static Dictionary<string, string> LoadDotEnv(string? path)
        {
            if (path == null) return new Dictionary<string, string>();
            return ParseDotEnv(File.ReadAllLines(path));
        }

        /// <summary>
        /// Parse baris format KEY=VALUE. Baris kosong dan komentar (#) dilewati;
        /// tanda kutip pembungkus nilai dibuang. Tidak ada komentar di tengah baris,
        /// supaya password yang mengandung '#' tetap utuh.
        /// </summary>
        internal static Dictionary<string, string> ParseDotEnv(IEnumerable<string> lines)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith('#')) continue;

                int eq = line.IndexOf('=');
                if (eq <= 0) continue;

                string key = line[..eq].Trim();
                string value = line[(eq + 1)..].Trim();

                if (value.Length >= 2 &&
                    ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
                {
                    value = value[1..^1];
                }

                result[key] = value;
            }

            return result;
        }
    }
}
