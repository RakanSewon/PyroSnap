using System;
using System.Diagnostics;
using System.Windows;
using PyroSnap.Services;

namespace PyroSnap
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Panggil fungsi test saat window selesai di-load
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Memulai inisialisasi DataSource...");

            // Ganti dengan MAP_KEY asli dari NASA yang dikirimkan via email
            string mapKey = "3726c0f3b209d53f165c5ab35ae57e28";

            // Menggunakan endpoint /country/csv/ untuk mengambil data MODIS_NRT negara Indonesia (IDN) 
            // selama 1 hari terakhir.
            string nasaUrl = $"https://firms.modaps.eosdis.nasa.gov/api/area/csv/{mapKey}/VIIRS_SNPP_NRT/105,-9,115,-5/1";

            var nasaSource = new HotspotDataSource("NASA FIRMS", nasaUrl);

            bool isSuccess = await nasaSource.SyncAsync();

            if (isSuccess)
            {
                Debug.WriteLine($"Sinkronisasi berhasil! Data terupdate pada: {nasaSource.WaktuSinkronisasi}");

                // Mengakses total data
                Debug.WriteLine($"Jumlah total hotspot di Jawa hari ini: {nasaSource.Hotspots.Count} titik.");

                // Mengakses detail data satu per satu
                if (nasaSource.Hotspots.Count > 0)
                {
                    var hotspotPertama = nasaSource.Hotspots[0]; // Ambil data index ke-0
                    Debug.WriteLine($"Titik panas pertama terdeteksi di Koordinat: {hotspotPertama.Latitude}, {hotspotPertama.Longitude}");
                }
                // --- UJI COBA FITUR REPORT ---
                var laporan = new HotspotReport("TXT");

                // 1. Masukkan data dari NASA ke dalam laporan
                laporan.MuatData(nasaSource.Hotspots);

                // 2. Generate string laporannya
                if (laporan.Generate())
                {
                    // 3. Ekspor ke file (akan tersimpan di folder bin/Debug/net8.0-windows/)
                    string pathFile = "Laporan_Hotspot_Jawa.txt";
                    laporan.ExportToFile(pathFile);

                    Debug.WriteLine($"Laporan berhasil diekspor menjadi file: {pathFile}");
                }
            }
        }
    }
}