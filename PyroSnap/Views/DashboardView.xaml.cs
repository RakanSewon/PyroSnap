using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// Tampilan utama: peta hotspot (Leaflet via WebView2) + cari lokasi + filter + daftar.
    /// Use case: View Hotspot Map, Select/Search Location, Filter Hotspots.
    ///
    /// FE-02: kode dipecah menjadi partial class —
    ///   DashboardView.xaml.cs  : siklus hidup halaman, tampilan per peran, tombol navigasi
    ///   DashboardView.Kpi.cs     : kartu KPI di atas peta
    ///   DashboardView.Data.cs    : muat data (NASA FIRMS / database), status, peringatan operasional
    ///   DashboardView.Historis.cs: data historis per rentang tanggal
    ///   DashboardView.Filter.cs  : cari lokasi dan filter
    ///   DashboardView.Warga.cs   : wilayah saya, banner peringatan warga, ringkasan per wilayah
    ///   DashboardView.Peta.cs    : peta WebView2/Leaflet dan fallback kanvas
    /// </summary>
    public partial class DashboardView : Page
    {
        private List<Hotspot> _semua = new List<Hotspot>();   // data yang sedang dipakai (terbaru atau historis)
        private List<Hotspot> _tampil = new List<Hotspot>();  // setelah filter + area

        // Satu sumber kriteria: filter (confidence, FRP) + area pencarian.
        // Daftar tampil selalu dihitung ulang dari _semua, jadi urutan cari/filter tidak berpengaruh.
        private FilterCriteria _filter = new FilterCriteria();
        private Location? _areaFokus;

        private bool _sudahDimuat;   // cegah Loaded berjalan dua kali
        private bool _sedangMemuat;

        public DashboardView()
        {
            InitializeComponent();
            Loaded += DashboardView_Loaded;
            // Setiap kunjungan membuat DashboardView baru (Navigasi, ganti tema); lepaskan proses
            // browser WebView2 halaman lama agar tidak menumpuk di memori.
            Unloaded += (_, _) =>
            {
                _petaDibuang = true;
                MapWeb.Dispose();
            };
        }

        private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_sudahDimuat) return;
            _sudahDimuat = true;

            // AUTH-09: dashboard hanya untuk sesi yang sah (login atau Tamu).
            if (!Navigasi.PastikanSesi(this)) return;

            AturTampilanPeran();
            IsiPilihanWilayah();

            try
            {
                await InitMapAsync();

                if (AppSession.DataDimuat)
                    PulihkanDariSesi();
                else
                    await MuatDataAsync();

                await MuatWilayahPilihanAsync();
                await PerbaruiJumlahAlertAsync();
            }
            catch (Exception ex)
            {
                TampilkanStatus("Terjadi kesalahan saat membuka dashboard.", PesanUi.Ringkas(ex.Message), JenisPesan.Error);
            }
        }

        // ---- RUI-03: elemen tampil sesuai peran (role-based-ui.md §2) ----
        private void AturTampilanPeran()
        {
            // Nama/peran, lonceng peringatan, Panduan, dan Keluar ada di shell (MainWindow).
            BtnReload.Visibility = Tampil(AppSession.BolehSinkron);
            BtnExport.Visibility = Tampil(AppSession.BolehEkspor);
            PanelHistoris.Visibility = Tampil(AppSession.BolehHistoris);
            PanelWilayahSaya.Visibility = Tampil(AppSession.IsWarga);
        }

        private static Visibility Tampil(bool ya) => ya ? Visibility.Visible : Visibility.Collapsed;

        // ---- Use case: View Hotspot Detail ----
        private void BtnDetail_Click(object sender, RoutedEventArgs e) => BukaDetail();

        private void GridHotspot_MouseDoubleClick(object sender, MouseButtonEventArgs e) => BukaDetail();

        // UX-16: Enter pada daftar membuka Detail.
        private void GridHotspot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            BukaDetail();
        }

        private void GridHotspot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridHotspot.SelectedItem is HotspotRow row)
                AppSession.SelectedHotspot = row.Source;

            if (_useWeb) SorotDiPeta();
            else GambarPeta(); // sorot titik terpilih pada kanvas
        }

        private void BukaDetail()
        {
            if (GridHotspot.SelectedItem is not HotspotRow row)
            {
                TampilkanStatus("Pilih salah satu titik panas dari daftar atau peta dulu.", "", JenisPesan.Peringatan);
                return;
            }

            AppSession.SelectedHotspot = row.Source;
            Navigasi.Ke(this, new DetailView());
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.BolehEkspor) Navigasi.Ke(this, new ExportView());
        }
    }
}
