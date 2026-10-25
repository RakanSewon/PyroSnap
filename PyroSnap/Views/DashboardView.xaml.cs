using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// Tampilan utama: peta hotspot (Leaflet/OSM via WebView2) + cari lokasi + filter + daftar.
    /// Mewakili use case: View Hotspot Map, Select/Search Location, Filter Hotspots.
    /// Jika WebView2 tidak tersedia, otomatis memakai peta representasi kanvas (offline).
    /// </summary>
    public partial class DashboardView : Page
    {
        private List<Hotspot> _semua = new List<Hotspot>();
        private List<Hotspot> _tampil = new List<Hotspot>();
        private Location? _areaFokus;

        private bool _useWeb;        // true bila WebView2 berhasil diinisialisasi
        private bool _webReady;      // true bila halaman map.html sudah memanggil MAP_READY

        private const string MapKey = "3726c0f3b209d53f165c5ab35ae57e28";
        private const string NasaUrl =
            "https://firms.modaps.eosdis.nasa.gov/api/area/csv/" + MapKey +
            "/VIIRS_SNPP_NRT/95,-11,141,6/1";

        private const double LonMin = 95, LonMax = 141, LatMin = -11, LatMax = 6;

        private static readonly List<Location> KnownLocations = new()
        {
            new Location("Riau",              0.5,   101.5),
            new Location("Jambi",            -1.6,   103.6),
            new Location("Sumatera Selatan", -3.0,   104.8),
            new Location("Kalimantan Barat", -0.03,  109.3),
            new Location("Kalimantan Tengah",-1.7,   113.4),
            new Location("Jawa",             -7.0,   110.0),
            new Location("Sulawesi",         -2.0,   120.0),
            new Location("Papua",            -4.0,   138.0),
        };

        public DashboardView()
        {
            InitializeComponent();
            Loaded += DashboardView_Loaded;
        }

        private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitMapAsync();
            await MuatDataAsync();
        }

        // ---- Inisialisasi WebView2 + Leaflet, dengan fallback ke kanvas ----
        private async System.Threading.Tasks.Task InitMapAsync()
        {
            try
            {
                await MapWeb.EnsureCoreWebView2Async(null);
                MapWeb.CoreWebView2.WebMessageReceived += Web_MessageReceived;

                // User-Agent browser normal agar penyedia tile tidak memblokir request.
                MapWeb.CoreWebView2.Settings.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

                string htmlPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Assets", "map.html");
                MapWeb.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
                _useWeb = true;
            }
            catch
            {
                // WebView2 Runtime tidak ada → pakai peta kanvas.
                _useWeb = false;
                MapWeb.Visibility = Visibility.Collapsed;
                CanvasMapHost.Visibility = Visibility.Visible;
            }
        }

        private void Web_MessageReceived(object? sender,
            Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            string msg = e.TryGetWebMessageAsString();

            if (msg == "MAP_READY")
            {
                _webReady = true;
                KirimDataKePeta();
                return;
            }

            // Pesan lain = index hotspot yang diklik di peta.
            if (int.TryParse(msg, out int idx) && idx >= 0 && idx < _tampil.Count)
            {
                var hs = _tampil[idx];
                var rows = GridHotspot.ItemsSource as IEnumerable<HotspotRow>;
                var row = rows?.FirstOrDefault(r => ReferenceEquals(r.Source, hs));
                if (row != null)
                {
                    GridHotspot.SelectedItem = row;
                    GridHotspot.ScrollIntoView(row);
                }
            }
        }

        private async System.Threading.Tasks.Task MuatDataAsync()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            LblMapStatus.Text = "Memuat data hotspot dari NASA FIRMS...";

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

            if (_semua.Count == 0)
            {
                _semua = DummyHotspots();
                LblMapStatus.Text = "API tidak tersedia — menampilkan data contoh.";
                await System.Threading.Tasks.Task.Delay(700);
            }

            LoadingOverlay.Visibility = Visibility.Collapsed;

            _areaFokus = null;
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
                new Hotspot(-7.2, 110.1, 55, 320, 18.0, now.AddHours(-6)),
                new Hotspot(-2.1, 120.3, 78, 340, 39.5, now.AddHours(-7)),
            };
        }

        private void RefreshTampilan()
        {
            GridHotspot.ItemsSource = _tampil.Select(h => new HotspotRow(h)).ToList();
            LblCount.Text = $"{_tampil.Count} dari {_semua.Count} titik";
            AppSession.CurrentHotspots = _tampil;

            if (_useWeb) KirimDataKePeta();
            else GambarPeta();

            if (GridHotspot.Items.Count > 0)
                GridHotspot.SelectedIndex = 0;
        }

        // Kirim data hotspot ke Leaflet sebagai JSON. Index dipakai sebagai id.
        private void KirimDataKePeta()
        {
            if (!_useWeb || !_webReady || MapWeb.CoreWebView2 == null) return;

            var sb = new StringBuilder("[");
            for (int i = 0; i < _tampil.Count; i++)
            {
                var h = _tampil[i];
                if (i > 0) sb.Append(',');
                sb.Append('{')
                  .Append($"\"id\":{i},")
                  .Append($"\"lat\":{h.Latitude.ToString(CultureInfo.InvariantCulture)},")
                  .Append($"\"lon\":{h.Longitude.ToString(CultureInfo.InvariantCulture)},")
                  .Append($"\"conf\":{h.Confidence},")
                  .Append($"\"frp\":{h.Frp.ToString(CultureInfo.InvariantCulture)}")
                  .Append('}');
            }
            sb.Append(']');

            // JSON di-encode lagi agar aman dilewatkan sebagai argumen string JS.
            string jsArg = System.Text.Json.JsonSerializer.Serialize(sb.ToString());
            _ = MapWeb.CoreWebView2.ExecuteScriptAsync($"setHotspots({jsArg})");
        }

        // ---- Use case: Select / Search Location ----
        private void BtnSearch_Click(object sender, RoutedEventArgs e) => CariLokasi();

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) CariLokasi();
        }

        private void CariLokasi()
        {
            string kata = TxtSearch.Text.Trim();
            if (string.IsNullOrEmpty(kata))
            {
                _areaFokus = null;
                _tampil = new List<Hotspot>(_semua);
                RefreshTampilan();
                return;
            }

            var lokasi = KnownLocations.FirstOrDefault(
                l => l.NamaLokasi.ToLower().Contains(kata.ToLower()));

            if (lokasi == null)
            {
                MessageBox.Show($"Lokasi \"{kata}\" tidak dikenal.\n" +
                    "Coba: Riau, Jambi, Sumatera Selatan, Kalimantan Barat/Tengah, Jawa, Sulawesi, Papua.",
                    "PyroSnap");
                return;
            }

            _areaFokus = lokasi;
            const double radius = 3.0;
            _tampil = _semua.Where(h =>
                Math.Abs(h.Latitude - lokasi.Latitude) <= radius &&
                Math.Abs(h.Longitude - lokasi.Longitude) <= radius).ToList();

            RefreshTampilan();

            // Arahkan peta ke lokasi tersebut.
            if (_useWeb && _webReady && MapWeb.CoreWebView2 != null)
            {
                string lat = lokasi.Latitude.ToString(CultureInfo.InvariantCulture);
                string lon = lokasi.Longitude.ToString(CultureInfo.InvariantCulture);
                _ = MapWeb.CoreWebView2.ExecuteScriptAsync($"focusLocation({lat},{lon},7)");
            }

            if (_tampil.Count == 0)
                MessageBox.Show($"Tidak ada hotspot di sekitar {lokasi.NamaLokasi}.", "PyroSnap");
        }

        // ---- Use case: Filter Hotspots ----
        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var filter = new FilterCriteria
            {
                ConfidenceMin = (int)SldConfidence.Value,
                FrpMin = SldFrp.Value
            };

            var basis = _areaFokus == null ? _semua : _tampil;
            _tampil = basis.Where(filter.IsMatch).ToList();
            RefreshTampilan();

            if (_tampil.Count == 0)
                MessageBox.Show("Tidak ada hotspot yang sesuai filter.", "PyroSnap");
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            SldConfidence.Value = 0;
            SldFrp.Value = 0;
            TxtSearch.Clear();
            _areaFokus = null;
            _tampil = new List<Hotspot>(_semua);
            RefreshTampilan();
        }

        private async void BtnReload_Click(object sender, RoutedEventArgs e) => await MuatDataAsync();

        private void SldConfidence_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LblConfidence != null) LblConfidence.Text = $"{(int)e.NewValue} %";
        }

        private void SldFrp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LblFrp != null) LblFrp.Text = $"{(int)e.NewValue} MW";
        }

        // ---- Use case: View Hotspot Detail ----
        private void BtnDetail_Click(object sender, RoutedEventArgs e) => BukaDetail();

        private void GridHotspot_MouseDoubleClick(object sender, MouseButtonEventArgs e) => BukaDetail();

        private void GridHotspot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_useWeb) GambarPeta(); // sorot titik terpilih pada kanvas
        }

        private void BukaDetail()
        {
            if (GridHotspot.SelectedItem is not HotspotRow row)
            {
                MessageBox.Show("Pilih salah satu hotspot dari daftar atau peta dulu.", "PyroSnap");
                return;
            }

            AppSession.SelectedHotspot = row.Source;
            ((MainWindow)Window.GetWindow(this)).Navigate(new DetailView());
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
            => ((MainWindow)Window.GetWindow(this)).Navigate(new ExportView());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
            => ((MainWindow)Window.GetWindow(this)).Navigate(new LoginView());

        // =========================================================
        //  Fallback peta kanvas (dipakai hanya bila WebView2 gagal)
        // =========================================================
        private void MapCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_useWeb) return;
            GambarGrid();
            GambarPeta();
        }

        private void GambarGrid()
        {
            GridCanvas.Children.Clear();
            double w = MapCanvas.ActualWidth, h = MapCanvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            var penBrush = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0));
            for (double lon = 100; lon < LonMax; lon += 10)
            {
                double x = (lon - LonMin) / (LonMax - LonMin) * w;
                GridCanvas.Children.Add(new Line { X1 = x, Y1 = 0, X2 = x, Y2 = h, Stroke = penBrush, StrokeThickness = 1 });
                GridCanvas.Children.Add(new TextBlock
                {
                    Text = lon + "°E", FontSize = 9, Foreground = penBrush,
                    RenderTransform = new TranslateTransform(x + 2, h - 14)
                });
            }
            for (double lat = 5; lat > LatMin; lat -= 5)
            {
                double y = (LatMax - lat) / (LatMax - LatMin) * h;
                GridCanvas.Children.Add(new Line { X1 = 0, Y1 = y, X2 = w, Y2 = y, Stroke = penBrush, StrokeThickness = 1 });
            }
        }

        private void GambarPeta()
        {
            if (MapCanvas == null) return;
            MapCanvas.Children.Clear();

            double w = MapCanvas.ActualWidth, h = MapCanvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            var dipilih = (GridHotspot.SelectedItem as HotspotRow)?.Source;

            foreach (var hs in _tampil)
            {
                double x = (hs.Longitude - LonMin) / (LonMax - LonMin) * w;
                double y = (LatMax - hs.Latitude) / (LatMax - LatMin) * h;

                bool isSel = ReferenceEquals(hs, dipilih);
                double r = (hs.Confidence >= 80 ? 11 : hs.Confidence >= 50 ? 9 : 7) + (isSel ? 4 : 0);
                Brush fill = hs.Confidence >= 80 ? new SolidColorBrush(Color.FromRgb(200, 30, 30))
                           : hs.Confidence >= 50 ? new SolidColorBrush(Color.FromRgb(230, 140, 0))
                           : new SolidColorBrush(Color.FromRgb(230, 200, 0));

                var dot = new Ellipse
                {
                    Width = r, Height = r, Fill = fill,
                    Stroke = isSel ? Brushes.Black : Brushes.White,
                    StrokeThickness = isSel ? 2.5 : 1.5,
                    Cursor = Cursors.Hand, Tag = hs,
                    ToolTip = $"{hs.Latitude:0.00}, {hs.Longitude:0.00}  |  {hs.Confidence}%  |  {hs.Frp} MW"
                };
                dot.MouseLeftButtonUp += Dot_Click;

                Canvas.SetLeft(dot, x - r / 2);
                Canvas.SetTop(dot, y - r / 2);
                MapCanvas.Children.Add(dot);
            }
        }

        private void Dot_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Ellipse el || el.Tag is not Hotspot hs) return;

            var rows = GridHotspot.ItemsSource as IEnumerable<HotspotRow>;
            var row = rows?.FirstOrDefault(r => ReferenceEquals(r.Source, hs));
            if (row == null) return;

            if (ReferenceEquals(GridHotspot.SelectedItem, row))
            {
                AppSession.SelectedHotspot = hs;
                ((MainWindow)Window.GetWindow(this)).Navigate(new DetailView());
            }
            else
            {
                GridHotspot.SelectedItem = row;
                GridHotspot.ScrollIntoView(row);
            }
        }
    }
}
