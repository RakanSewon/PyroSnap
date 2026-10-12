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
            }
        }
    }
}