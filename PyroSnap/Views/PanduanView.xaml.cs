using System.Windows;
using System.Windows.Controls;

namespace PyroSnap.Views
{
    /// <summary>
    /// PUB-03 / SCR-10: panduan & kontak darurat. Bisa dibuka tanpa login (FK5).
    /// Kembali ke Dashboard bila ada sesi, selain itu ke halaman Masuk.
    /// </summary>
    public partial class PanduanView : Page
    {
        public PanduanView()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Navigasi.KeBeranda(this);
    }
}
