using System.Windows;
using System.Windows.Controls;

namespace PyroSnap.Views
{
    /// <summary>
    /// PUB-03 / SCR-10: panduan & kontak darurat. Bisa dibuka tanpa login (FK5).
    /// Dengan sesi tampil di dalam shell (menu samping); tanpa sesi tampil penuh
    /// dengan bilah sendiri dan tombol kembali ke halaman Masuk.
    /// </summary>
    public partial class PanduanView : Page
    {
        public PanduanView()
        {
            InitializeComponent();
            if (!AppSession.AdaSesi)
            {
                BilahTanpaSesi.Visibility = Visibility.Visible;
                Gulir.Margin = new Thickness(16);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Navigasi.KeBeranda(this);
    }
}
