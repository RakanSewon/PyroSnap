using System.Windows;
using System.Windows.Controls;
using PyroSnap.Views;

namespace PyroSnap
{
    /// <summary>
    /// Shell aplikasi. Bertindak sebagai host navigasi antar-tampilan (Page).
    /// Memenuhi syarat Modul 7: "pastikan perpindahan antar form sudah bisa dilakukan".
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Hapus riwayat navigasi Frame setiap pindah halaman: halaman lama
            // (termasuk dashboard setelah logout) tidak bisa dibuka lagi lewat
            // tombol Back/Alt+←, dan instance-nya tidak menumpuk di memori.
            MainFrame.Navigated += (_, _) =>
            {
                while (MainFrame.CanGoBack) MainFrame.RemoveBackEntry();
            };

            // Tampilan pertama: Login.
            Navigate(new LoginView());
        }

        /// <summary>
        /// Berpindah ke sebuah tampilan (Page). Dipanggil dari tiap view.
        /// </summary>
        public void Navigate(Page page)
        {
            MainFrame.Navigate(page);
        }
    }
}
