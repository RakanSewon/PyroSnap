using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PyroSnap.Views
{
    /// <summary>
    /// Tampilan detail satu hotspot (use case View Hotspot Detail).
    /// Membaca hotspot terpilih dari AppSession.
    /// </summary>
    public partial class DetailView : Page
    {
        public DetailView()
        {
            InitializeComponent();
            TampilkanDetail();
        }

        private void TampilkanDetail()
        {
            var h = AppSession.SelectedHotspot;
            if (h == null) return;

            string tingkat = h.Confidence >= 80 ? "Tinggi"
                           : h.Confidence >= 50 ? "Sedang" : "Rendah";

            LblTingkat.Text = "Tingkat Keparahan: " + tingkat;
            LblTingkat.Foreground = h.Confidence >= 80 ? new SolidColorBrush(Color.FromRgb(183, 28, 28))
                                  : h.Confidence >= 50 ? new SolidColorBrush(Color.FromRgb(230, 81, 0))
                                  : new SolidColorBrush(Color.FromRgb(191, 160, 15));

            LblKoordinat.Text = $"{h.Latitude:0.0000}, {h.Longitude:0.0000}";
            LblConfidence.Text = h.Confidence + " %";
            LblBrightness.Text = h.Brightness + " K";
            LblFrp.Text = h.Frp + " MW";
            LblWaktu.Text = h.WaktuAkuisisi.ToString("dd MMM yyyy, HH:mm");
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Window.GetWindow(this)).Navigate(new DashboardView());
        }
    }
}
