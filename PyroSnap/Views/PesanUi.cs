using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PyroSnap.Views
{
    public enum JenisPesan { Info, Peringatan, Error }

    /// <summary>
    /// FE-05 / UX-02: pesan di tempat (bilah berwarna) menggantikan MessageBox untuk hal rutin.
    /// MessageBox tetap dipakai untuk konfirmasi hasil aksi dan error kritis.
    /// </summary>
    public static class PesanUi
    {
        public const int PanjangMaks = 160;

        /// <summary>Potong pesan teknis yang terlalu panjang.</summary>
        public static string Ringkas(string pesan) =>
            pesan.Length > PanjangMaks ? pesan[..PanjangMaks] + "…" : pesan;

        public static void Tampilkan(Border bilah, TextBlock teks, string pesan, JenisPesan jenis)
        {
            (string bg, string fg) = jenis switch
            {
                JenisPesan.Error => ("PesanErrorBg", "PesanErrorFg"),
                JenisPesan.Peringatan => ("PesanPeringatanBg", "PesanPeringatanFg"),
                _ => ("PesanInfoBg", "PesanInfoFg")
            };

            bilah.Background = (Brush)Application.Current.FindResource(bg);
            teks.Foreground = (Brush)Application.Current.FindResource(fg);
            teks.FontWeight = jenis == JenisPesan.Info ? FontWeights.Normal : FontWeights.SemiBold;
            teks.Text = pesan;
            bilah.Visibility = Visibility.Visible;
        }

        public static void Sembunyikan(Border bilah) => bilah.Visibility = Visibility.Collapsed;
    }
}
