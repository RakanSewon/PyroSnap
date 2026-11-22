using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PyroSnap.Views
{
    public enum JenisPesan { Info, Peringatan, Error }

    /// <summary>
    /// FE-05 / UX-02: pesan di tempat (bilah berwarna) menggantikan MessageBox untuk hal rutin.
    /// DS-09: bila bilah berisi TextBlock ber-Tag "ikon", ikonnya diisi sesuai jenis pesan.
    /// Warna diambil dari tema aktif saat pesan ditampilkan.
    /// </summary>
    public static class PesanUi
    {
        public const int PanjangMaks = 160;

        /// <summary>Potong pesan teknis yang terlalu panjang.</summary>
        public static string Ringkas(string pesan) =>
            pesan.Length > PanjangMaks ? pesan[..PanjangMaks] + "…" : pesan;

        public static void Tampilkan(Border bilah, TextBlock teks, string pesan, JenisPesan jenis)
        {
            (string bg, string fg, string glyph) = jenis switch
            {
                JenisPesan.Error => ("PesanErrorBg", "PesanErrorFg", Ikon.Error),
                JenisPesan.Peringatan => ("PesanPeringatanBg", "PesanPeringatanFg", Ikon.Peringatan),
                _ => ("PesanInfoBg", "PesanInfoFg", Ikon.Info)
            };

            var warnaTeks = (Brush)Application.Current.FindResource(fg);
            bilah.Background = (Brush)Application.Current.FindResource(bg);
            teks.Foreground = warnaTeks;
            teks.FontWeight = jenis == JenisPesan.Info ? FontWeights.Normal : FontWeights.SemiBold;
            teks.Text = pesan;

            if (CariIkon(bilah) is { } ikon)
            {
                ikon.Text = glyph;
                ikon.Foreground = warnaTeks;
            }

            bilah.Visibility = Visibility.Visible;
        }

        public static void Sembunyikan(Border bilah) => bilah.Visibility = Visibility.Collapsed;

        private static TextBlock? CariIkon(DependencyObject induk) =>
            LogicalTreeHelper.GetChildren(induk).OfType<DependencyObject>()
                .Select(anak => anak is TextBlock { Tag: "ikon" } tb ? tb : CariIkon(anak))
                .FirstOrDefault(tb => tb != null);
    }
}
