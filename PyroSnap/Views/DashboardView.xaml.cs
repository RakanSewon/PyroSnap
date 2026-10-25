using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// Tampilan utama: peta hotspot + filter + daftar.
    /// Mewakili use case View Hotspot Map &amp; Filter Hotspots.
    /// Mengambil data lewat HotspotDataSource (NASA FIRMS) milik tim,
    /// dengan fallback dummy bila API tidak tersedia.
    /// </summary>
    public partial class DashboardView : Page
    {
        private List<Hotspot> _semua = new List<Hotspot>();
        private List<Hotspot> _tampil = new List<Hotspot>();

        // Endpoint sama seperti yang dipakai tim di MainWindow lama.
        private const string MapKey = "3726c0f3b209d53f165c5ab35ae57e28";
        private const string NasaUrl =
            "https://firms.modaps.eosdis.nasa.gov/api/area/csv/" + MapKey +
            "/VIIRS_SNPP_NRT/95,-11,141,6/1";

        public DashboardView()
        {
            InitializeComponent();
            Loaded += async (_, _) => await MuatDataAsync();
        }

        private async System.Threading.Tasks.Task MuatDataAsync()
        {
            LblMapStatus.Text = "Memuat data hotspot...";
            LblMapStatus.Visibility = Visibility.Visible;

            try
            {
                var source = new HotspotDataSource("NASA FIRMS", NasaUrl);
                await source.SyncAsync();
                _semua = source.Hotspots ?? new List<Hotspot>();
            }
            catch
            {
                _semua = new List<Hotspot>();
            }

            // Fallback demo bila kosong, agar tampilan tetap bisa dinilai offline.
            if (_semua.Count == 0)
            {
                _semua = DummyHotspots();
                LblMapStatus.Text = "Menampilkan data contoh (API tidak tersedia).";
            }
            else
            {
                LblMapStatus.Visibility = Visibility.Collapsed;
            }

            _tampil = new List<Hotspot>(_semua);
            RefreshTampilan();
        }

        private static List<Hotspot> DummyHotspots()
        {
            var now = DateTime.Now;
            return new List<Hotspot>
            {
                new Hotspot(0.5071, 101.4478, 91, 342, 48.5, now.AddHours(-2)),
                new Hotspot(-0.0263, 109.3425, 74, 318, 22.1, now.AddHours(-5)),
                new Hotspot(-2.9909, 104.7566, 88, 356, 61.3, now.AddHours(-1)),
                new Hotspot(-1.6101, 103.6131, 42, 305, 11.7, now.AddHours(-8)),
                new Hotspot(-1.6815, 113.3824, 67, 333, 34.9, now.AddHours(-3)),
                new Hotspot(0.2933, 101.7068, 83, 349, 52.0, now.AddHours(-4)),
            };
        }

        private void RefreshTampilan()
        {
            GridHotspot.ItemsSource = _tampil.Select(h => new HotspotRow(h)).ToList();
            LblCount.Text = $"{_tampil.Count} dari {_semua.Count} titik";
            AppSession.CurrentHotspots = _tampil;
            GambarPeta();
        }

        // ---- Filter (use case Filter Hotspots), memakai FilterCriteria milik tim ----
        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var filter = new FilterCriteria
            {
                ConfidenceMin = (int)SldConfidence.Value,
                FrpMin = SldFrp.Value
            };

            _tampil = _semua.Where(filter.IsMatch).ToList();
            RefreshTampilan();

            if (_tampil.Count == 0)
                MessageBox.Show("Tidak ada hotspot yang sesuai filter.", "PyroSnap");
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            SldConfidence.Value = 0;
            SldFrp.Value = 0;
            _tampil = new List<Hotspot>(_semua);
            RefreshTampilan();
        }

        private async void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            await MuatDataAsync();
        }

        private void SldConfidence_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LblConfidence != null) LblConfidence.Text = $"{(int)e.NewValue} %";
        }

        private void SldFrp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LblFrp != null) LblFrp.Text = $"{(int)e.NewValue} MW";
        }

        // ---- Navigasi ke Detail (use case View Hotspot Detail) ----
        private void BtnDetail_Click(object sender, RoutedEventArgs e)
        {
            BukaDetail();
        }

        private void GridHotspot_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BukaDetail();
        }

        private void BukaDetail()
        {
            if (GridHotspot.SelectedItem is not HotspotRow row)
            {
                MessageBox.Show("Pilih salah satu hotspot dari daftar dulu.", "PyroSnap");
                return;
            }

            AppSession.SelectedHotspot = row.Source;
            ((MainWindow)Window.GetWindow(this)).Navigate(new DetailView());
        }

        // ---- Navigasi ke Export (use case Export Hotspot Report) ----
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Window.GetWindow(this)).Navigate(new ExportView());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Window.GetWindow(this)).Navigate(new LoginView());
        }

        private void MapCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GambarPeta(); // gambar ulang saat ukuran canvas berubah / setelah layout pertama
        }

        // ---- Menggambar titik hotspot pada "peta" ----
        private void GambarPeta()
        {
            MapCanvas.Children.Clear();

            double w = MapCanvas.ActualWidth;
            double h = MapCanvas.ActualHeight;
            if (w <= 0 || h <= 0) return; // belum ter-layout; akan digambar saat refresh berikutnya

            const double lonMin = 95, lonMax = 141;
            const double latMin = -11, latMax = 6;

            foreach (var hs in _tampil)
            {
                double x = (hs.Longitude - lonMin) / (lonMax - lonMin) * w;
                double y = (latMax - hs.Latitude) / (latMax - latMin) * h;

                double r = hs.Confidence >= 80 ? 10 : hs.Confidence >= 50 ? 8 : 6;
                Brush fill = hs.Confidence >= 80 ? new SolidColorBrush(Color.FromRgb(200, 30, 30))
                           : hs.Confidence >= 50 ? new SolidColorBrush(Color.FromRgb(230, 140, 0))
                           : new SolidColorBrush(Color.FromRgb(230, 200, 0));

                var dot = new Ellipse
                {
                    Width = r,
                    Height = r,
                    Fill = fill,
                    Stroke = Brushes.White,
                    StrokeThickness = 1.5,
                    ToolTip = new HotspotRow(hs).Koordinat + "  |  " + hs.Confidence + "%"
                };
                Canvas.SetLeft(dot, x - r / 2);
                Canvas.SetTop(dot, y - r / 2);
                MapCanvas.Children.Add(dot);
            }
        }
    }
}
