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
