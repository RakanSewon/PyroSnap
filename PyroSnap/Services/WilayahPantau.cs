using System;
using System.Collections.Generic;
using System.Linq;

namespace PyroSnap.Services
{
    /// <summary>
    /// Daftar area pantau yang dipakai pencarian lokasi, wilayah pilihan warga, dan
    /// ringkasan risiko (keputusan sementara K2, 2026-11-22): sebuah hotspot masuk area
    /// bila berada dalam kotak ±<see cref="RadiusDerajat"/>° dari pusat area; bila masuk
    /// beberapa area, dipilih pusat terdekat. Ini perkiraan, bukan batas administratif.
    /// </summary>
    public static class WilayahPantau
    {
        public const double RadiusDerajat = 3.0;

        public static IReadOnlyList<Location> Semua { get; } = new List<Location>
        {
            new Location("Riau",              0.5,   101.5),
            new Location("Jambi",            -1.6,   103.6),
            new Location("Sumatera Selatan", -3.0,   104.8),
            new Location("Kalimantan Barat", -0.03,  109.3),
            new Location("Kalimantan Tengah",-1.7,   113.4),
            new Location("Jawa",             -7.0,   110.0),
            new Location("Sulawesi",         -2.0,   120.0),
            new Location("Papua",            -4.0,   138.0),
        };

        /// <summary>Area pertama yang namanya mengandung kata kunci (tidak peka huruf besar).</summary>
        public static Location? Cari(string kata) =>
            Semua.FirstOrDefault(l => l.NamaLokasi.Contains(kata.Trim(), StringComparison.OrdinalIgnoreCase));

        /// <summary>Area dengan nama persis sama (tidak peka huruf besar).</summary>
        public static Location? CariPersis(string? nama) =>
            nama == null ? null
                : Semua.FirstOrDefault(l => string.Equals(l.NamaLokasi, nama.Trim(), StringComparison.OrdinalIgnoreCase));

        public static bool DalamArea(Hotspot h, Location area) =>
            Math.Abs(h.Latitude - area.Latitude) <= RadiusDerajat &&
            Math.Abs(h.Longitude - area.Longitude) <= RadiusDerajat;

        /// <summary>Area terdekat yang mencakup hotspot; null bila di luar semua area.</summary>
        public static Location? Temukan(Hotspot h) =>
            Semua.Where(a => DalamArea(h, a))
                 .OrderBy(a => Math.Pow(h.Latitude - a.Latitude, 2) + Math.Pow(h.Longitude - a.Longitude, 2))
                 .FirstOrDefault();
    }
}
