using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    // Bagian Dashboard: peta Leaflet (WebView2) dan fallback peta kanvas.
    public partial class DashboardView
    {
        private bool _fitPeta = true; // zoom ke semua titik hanya setelah data baru dimuat
        private bool _useWeb;         // true bila WebView2 berhasil diinisialisasi
        private bool _webReady;       // true bila map.html sudah memanggil MAP_READY

        private const double LonMin = 95, LonMax = 141, LatMin = -11, LatMax = 6;

        // ---- Inisialisasi WebView2 + Leaflet, dengan fallback ke kanvas ----
        private async Task InitMapAsync()
        {
            IsiLegendaKanvas();

            try
            {
                // DS-08: latar WebView sama dengan latar peta tema (tanpa kilatan putih di tema gelap).
                MapWeb.DefaultBackgroundColor = WarnaGambar(Tema.Warna("MapBg"));
                await MapWeb.EnsureCoreWebView2Async(null);
                MapWeb.CoreWebView2.WebMessageReceived += Web_MessageReceived;

                // User-Agent browser normal agar penyedia tile tidak memblokir request.
                MapWeb.CoreWebView2.Settings.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

                // Konfigurasi peta disuntikkan sebelum map.html berjalan: key tile CARTO (opsional)
                // dan aturan tingkat (FE-01), supaya tidak ditulis ulang di HTML.
                string mapConfig = JsonSerializer.Serialize(new
                {
                    cartoKey = AppConfig.CartoKey,
                    tingkat = TingkatHotspot.KonfigurasiPeta(),
                    tema = Tema.KonfigurasiPeta()
                });
                await MapWeb.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                    $"window.PYROSNAP_CONFIG = {mapConfig};");

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
                if (_areaFokus != null) FokuskanPeta(_areaFokus);
                return;
            }

            // Pesan lain = index hotspot yang diklik di peta.
            if (int.TryParse(msg, out int idx) && idx >= 0 && idx < _tampil.Count)
            {
                var row = CariBaris(_tampil[idx]);
                if (row != null)
                {
                    GridHotspot.SelectedItem = row;
                    GridHotspot.ScrollIntoView(row);
                }
            }
        }

        // Kirim data hotspot ke Leaflet sebagai JSON. Index dipakai sebagai id.
        private void KirimDataKePeta()
        {
            if (!_useWeb || !_webReady || MapWeb.CoreWebView2 == null) return;

            string json = JsonSerializer.Serialize(_tampil.Select((h, i) => new
            {
                id = i,
                lat = h.Latitude,
                lon = h.Longitude,
                conf = h.Confidence,
                frp = h.Frp
            }));

            // JSON di-encode lagi agar aman dilewatkan sebagai argumen string JS.
            string jsArg = JsonSerializer.Serialize(json);
            string fit = _fitPeta ? "true" : "false";
            _fitPeta = false;
            _ = MapWeb.CoreWebView2.ExecuteScriptAsync($"setHotspots({jsArg}, {fit})");

            SorotDiPeta();
        }

        private void SorotDiPeta()
        {
            if (!_useWeb || !_webReady || MapWeb.CoreWebView2 == null) return;

            int idx = GridHotspot.SelectedItem is HotspotRow row ? _tampil.IndexOf(row.Source) : -1;
            _ = MapWeb.CoreWebView2.ExecuteScriptAsync($"highlightHotspot({idx})");
        }

        private void FokuskanPeta(Location lokasi)
        {
            if (!_useWeb || !_webReady || MapWeb.CoreWebView2 == null) return;

            string lat = lokasi.Latitude.ToString(CultureInfo.InvariantCulture);
            string lon = lokasi.Longitude.ToString(CultureInfo.InvariantCulture);
            _ = MapWeb.CoreWebView2.ExecuteScriptAsync($"focusLocation({lat},{lon},7)");
        }

        // =========================================================
        //  Fallback peta kanvas (dipakai hanya bila WebView2 gagal)
        // =========================================================
        private void IsiLegendaKanvas()
        {
            LegendaKanvas.Children.Clear();
            foreach (var t in new[] { Tingkat.Tinggi, Tingkat.Nominal, Tingkat.Rendah })
            {
                var baris = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 1, 0, 1) };
                baris.Children.Add(new Grid
                {
                    Width = 12, Height = 12, VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new Ellipse
                        {
                            Width = TingkatHotspot.UkuranTitik(t), Height = TingkatHotspot.UkuranTitik(t),
                            Fill = TingkatHotspot.Warna(t), Stroke = TingkatHotspot.Tepi(t),
                            StrokeThickness = TingkatHotspot.TebalTepi(t),
                            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                });
                baris.Children.Add(new TextBlock
                {
                    Text = $"{TingkatHotspot.Nama(t)} ({TingkatHotspot.Rentang(t)})",
                    FontSize = 12, Margin = new Thickness(6, 0, 0, 0)
                });
                LegendaKanvas.Children.Add(baris);
            }
        }

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

            // Garis lintang/bujur memakai warna teks tema dengan transparansi (terlihat di terang & gelap).
            Color teks = Tema.Warna("TextPrimary");
            var penBrush = new SolidColorBrush(Color.FromArgb(48, teks.R, teks.G, teks.B));
            var labelBrush = new SolidColorBrush(Tema.Warna("MutedText"));
            for (double lon = 100; lon < LonMax; lon += 10)
            {
                double x = (lon - LonMin) / (LonMax - LonMin) * w;
                GridCanvas.Children.Add(new Line { X1 = x, Y1 = 0, X2 = x, Y2 = h, Stroke = penBrush, StrokeThickness = 1 });
                GridCanvas.Children.Add(new TextBlock
                {
                    Text = lon + "°E", FontSize = 10, Foreground = labelBrush,
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

                Tingkat t = TingkatHotspot.Dari(hs.Confidence);
                bool isSel = ReferenceEquals(hs, dipilih);
                double r = TingkatHotspot.UkuranTitik(t) + (isSel ? 4 : 0);

                var dot = new Ellipse
                {
                    Width = r, Height = r, Fill = TingkatHotspot.Warna(t),
                    // DS-05 / UX-13: ukuran + garis tepi (Tinggi: tepi tebal); terpilih: tepi warna tinta tema
                    Stroke = isSel ? (Brush)FindResource("InkBg") : TingkatHotspot.Tepi(t),
                    StrokeThickness = isSel ? 3 : TingkatHotspot.TebalTepi(t),
                    Cursor = Cursors.Hand, Tag = hs,
                    ToolTip = $"{hs.Latitude:0.00}, {hs.Longitude:0.00} | {TingkatHotspot.Nama(t)} {hs.Confidence}% | FRP {hs.Frp:0.##} MW"
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

            var row = CariBaris(hs);
            if (row == null) return;

            if (ReferenceEquals(GridHotspot.SelectedItem, row))
            {
                AppSession.SelectedHotspot = hs;
                Navigasi.Ke(this, new DetailView());
            }
            else
            {
                GridHotspot.SelectedItem = row;
                GridHotspot.ScrollIntoView(row);
            }
        }

        private static System.Drawing.Color WarnaGambar(Color c) => System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
    }
}
