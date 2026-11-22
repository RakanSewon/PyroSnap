using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PyroSnap.Services
{
    /// <summary>
    /// Ambang kritis pemicu alert (ALR-01, keputusan K5 2026-11-22), di satu tempat.
    ///
    /// Acuan: kelas tingkat kepercayaan LAPAN (Panduan LAPAN Fire Hotspot v2.0, 2020):
    /// rendah 0–29, nominal 30–79, tinggi 80–100; kelas tinggi = "segera penanggulangan".
    /// VIIRS memberi kelas l/n/h; parser memetakan h → 90, jadi kelas tinggi = confidence ≥ 80.
    /// FRP tidak dipakai sebagai syarat (tidak ada ambang standar di literatur),
    /// hanya dicantumkan di pesan sebagai informasi intensitas.
    /// </summary>
    public static class AmbangKritis
    {
        public const int ConfidenceMin = 80;

        /// <summary>Batas alert baru per sinkronisasi (ALR-07); diambil yang FRP-nya terbesar.</summary>
        public const int MaksAlertPerSinkron = 50;

        public static bool Terlampaui(Hotspot h) => h.Confidence >= ConfidenceMin;

        /// <summary>
        /// Pilih hotspot kritis dan siapkan alert-nya (Alert.SendAlert), maksimal
        /// <see cref="MaksAlertPerSinkron"/>, urut FRP terbesar.
        /// </summary>
        public static List<(Hotspot Hotspot, Alert Alert)> BuatAlert(IEnumerable<Hotspot> hotspots)
        {
            return hotspots
                .Where(Terlampaui)
                .OrderByDescending(h => h.Frp)
                .Take(MaksAlertPerSinkron)
                .Select(h =>
                {
                    var alert = new Alert { Message = BuatPesan(h) };
                    alert.SendAlert("semua user");
                    return (h, alert);
                })
                .ToList();
        }

        /// <summary>Pesan singkat (kolom message VARCHAR(255)).</summary>
        public static string BuatPesan(Hotspot h) => string.Create(CultureInfo.InvariantCulture,
            $"Hotspot kepercayaan tinggi ({h.Confidence}%) di {h.Latitude:0.000}, {h.Longitude:0.000} · " +
            $"FRP {h.Frp:0.##} MW · {h.WaktuAkuisisi:dd MMM yyyy HH:mm} UTC · segera penanggulangan");
    }
}
