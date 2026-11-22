using System.Windows;
using System.Windows.Media;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    public enum Tingkat { Rendah, Nominal, Tinggi }

    /// <summary>
    /// FE-01 / UX-03: satu sumber aturan tingkat keyakinan deteksi, sesuai kelas LAPAN
    /// (Panduan LAPAN Fire Hotspot v2.0): Rendah 0–29, Nominal 30–79, Tinggi 80–100.
    /// Dipakai daftar, Detail, peta kanvas, dan dikirim ke map.html.
    /// </summary>
    public static class TingkatHotspot
    {
        public const int BatasNominal = 30;
        public const int BatasTinggi = AmbangKritis.ConfidenceMin; // 80, sama dengan pemicu alert

        public static Tingkat Dari(int confidence) =>
            confidence >= BatasTinggi ? Tingkat.Tinggi
            : confidence >= BatasNominal ? Tingkat.Nominal
            : Tingkat.Rendah;

        public static string Nama(Tingkat t) => t switch
        {
            Tingkat.Tinggi => "Tinggi",
            Tingkat.Nominal => "Nominal",
            _ => "Rendah"
        };

        public static string Rentang(Tingkat t) => t switch
        {
            Tingkat.Tinggi => $"≥ {BatasTinggi}%",
            Tingkat.Nominal => $"{BatasNominal}–{BatasTinggi - 1}%",
            _ => $"< {BatasNominal}%"
        };

        /// <summary>Penjelasan bahasa awam (UX-18).</summary>
        public static string Penjelasan(Tingkat t) => t switch
        {
            Tingkat.Tinggi => "Satelit sangat yakin ada sumber panas/api di titik ini. LAPAN: segera ditindaklanjuti.",
            Tingkat.Nominal => "Satelit cukup yakin ada sumber panas. LAPAN: waspada.",
            _ => "Keyakinan satelit rendah; bisa jadi bukan api. LAPAN: perlu diperhatikan."
        };

        public static string KunciWarna(Tingkat t) => t switch
        {
            Tingkat.Tinggi => "TingkatTinggi",
            Tingkat.Nominal => "TingkatNominal",
            _ => "TingkatRendah"
        };

        public static Brush Warna(Tingkat t) => (Brush)Application.Current.FindResource(KunciWarna(t));

        /// <summary>Diameter titik di peta kanvas (UX-13: tingkat tidak hanya dibedakan warna).</summary>
        public static double UkuranTitik(Tingkat t) => t switch
        {
            Tingkat.Tinggi => 11,
            Tingkat.Nominal => 9,
            _ => 7
        };

        private static string Hex(Tingkat t) =>
            Warna(t) is SolidColorBrush b ? $"#{b.Color.R:X2}{b.Color.G:X2}{b.Color.B:X2}" : "#808080";

        /// <summary>Konfigurasi untuk map.html (window.PYROSNAP_CONFIG.tingkat).</summary>
        public static object KonfigurasiPeta() => new
        {
            batasNominal = BatasNominal,
            batasTinggi = BatasTinggi,
            warna = new { tinggi = Hex(Tingkat.Tinggi), nominal = Hex(Tingkat.Nominal), rendah = Hex(Tingkat.Rendah) },
            label = new
            {
                tinggi = $"Tinggi ({Rentang(Tingkat.Tinggi)})",
                nominal = $"Nominal ({Rentang(Tingkat.Nominal)})",
                rendah = $"Rendah ({Rentang(Tingkat.Rendah)})"
            }
        };
    }
}
