using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// Detail satu titik panas (use case View Hotspot Detail) + verifikasi lapangan (OPS-01)
    /// dan navigasi ke titik (OPS-02) sesuai peran.
    /// </summary>
    public partial class DetailView : Page
    {
        private readonly Func<Page>? _halamanKembali;
        private Hotspot? _hotspot;

        /// <param name="halamanKembali">Halaman tujuan tombol Kembali; null = Dashboard.</param>
        public DetailView(Func<Page>? halamanKembali = null)
        {
            InitializeComponent();
            _halamanKembali = halamanKembali;
            Loaded += async (_, _) => await MuatAsync();
        }

        private static string NamaSumber =>
            AppSession.NamaSumber ?? HotspotDataSource.NamaFirms(AppConfig.FirmsSource);

        private async Task MuatAsync()
        {
            if (!Navigasi.PastikanSesi(this)) return;

            _hotspot = AppSession.SelectedHotspot;
            if (_hotspot == null)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan, "Tidak ada titik panas yang dipilih.", JenisPesan.Peringatan);
                return;
            }

            TampilkanData(_hotspot);
            PanelNavigasi.Visibility = AppSession.BolehNavigasi ? Visibility.Visible : Visibility.Collapsed;
            PanelFormVerifikasi.Visibility = AppSession.BolehVerifikasi ? Visibility.Visible : Visibility.Collapsed;
            await MuatVerifikasiAsync();
        }

        private void TampilkanData(Hotspot h)
        {
            Tingkat t = TingkatHotspot.Dari(h.Confidence);
            DotTingkat.Fill = TingkatHotspot.Warna(t);
            // Teks tetap warna gelap bawaan (kontras cukup); warna tingkat hanya di titik (A11Y-02).
            LblTingkat.Text = $"Keyakinan deteksi: {TingkatHotspot.Nama(t)} ({h.Confidence}%)";
            LblTingkatPenjelasan.Text = TingkatHotspot.Penjelasan(t);

            LblWilayah.Text = WilayahPantau.Temukan(h) is { } area
                ? $"Sekitar {area.NamaLokasi} (perkiraan area pantau)"
                : "Di luar area pantau";
            LblKoordinat.Text = KoordinatTeks(h);
            LblWaktu.Text = $"{h.WaktuAkuisisi.ToLocalTime():dd MMM yyyy, HH:mm} (waktu lokal) · " +
                            $"{h.WaktuAkuisisi:HH:mm} UTC";
            LblConfidence.Text = $"{h.Confidence} % — kelas {TingkatHotspot.Nama(t)} ({TingkatHotspot.Rentang(t)})";
            LblFrp.Text = $"{h.Frp:0.##} MW (makin besar, makin kuat apinya)";
            LblBrightness.Text = $"{h.Brightness:0.#} K";
        }

        private static string KoordinatTeks(Hotspot h) =>
            string.Create(CultureInfo.InvariantCulture, $"{h.Latitude:0.00000}, {h.Longitude:0.00000}");

        // ---- OPS-01: verifikasi lapangan ----
        private async Task MuatVerifikasiAsync()
        {
            if (_hotspot == null) return;

            try
            {
                var riwayat = await VerifikasiRepository.RiwayatAsync(NamaSumber, _hotspot);
                if (riwayat.Count == 0)
                {
                    LblVerifikasiStatus.Text = "Belum ada hasil cek lapangan untuk titik ini.";
                    ListRiwayat.ItemsSource = null;
                    return;
                }

                Verifikasi terbaru = riwayat[0];
                LblVerifikasiStatus.Text = $"Status terakhir: {terbaru.LabelStatus} " +
                                           $"({terbaru.Waktu.ToLocalTime():dd MMM yyyy HH:mm})" +
                                           (string.IsNullOrEmpty(terbaru.Catatan) ? "" : $" — {terbaru.Catatan}");
                LblVerifikasiStatus.FontWeight = FontWeights.SemiBold;

                // Nama pemeriksa hanya untuk petugas; warga cukup melihat status.
                ListRiwayat.ItemsSource = riwayat.Select(v =>
                    $"{v.Waktu.ToLocalTime():dd MMM yyyy HH:mm} · {v.LabelStatus}" +
                    (AppSession.BolehVerifikasi ? $" · oleh {v.Pemeriksa ?? "(akun dihapus)"}" : "") +
                    (string.IsNullOrEmpty(v.Catatan) ? "" : $" · {v.Catatan}")).ToList();
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                LblVerifikasiStatus.Text = "Status verifikasi tidak bisa dimuat: database tidak tersedia.";
                PanelFormVerifikasi.IsEnabled = false;
            }
        }

        private async void BtnSimpanVerifikasi_Click(object sender, RoutedEventArgs e)
        {
            if (!AppSession.BolehVerifikasi || _hotspot == null || AppSession.UserIdAngka is not int userId) return;

            string? status = RbApiAktif.IsChecked == true ? Verifikasi.ApiAktif
                           : RbSudahPadam.IsChecked == true ? Verifikasi.SudahPadam
                           : RbBukanKebakaran.IsChecked == true ? Verifikasi.BukanKebakaran
                           : null;
            if (status == null)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan, "Pilih status hasil cek lapangan dulu.", JenisPesan.Peringatan);
                return;
            }

            BtnSimpanVerifikasi.IsEnabled = false;
            try
            {
                bool tersimpan = await VerifikasiRepository.SimpanAsync(NamaSumber, _hotspot, userId, status, TxtCatatan.Text);
                if (!tersimpan)
                {
                    PesanUi.Tampilkan(BarPesan, LblPesan,
                        "Titik ini belum tersimpan di database, jadi belum bisa diverifikasi. Minta Admin memperbarui data.",
                        JenisPesan.Peringatan);
                    return;
                }

                TxtCatatan.Clear();
                RbApiAktif.IsChecked = RbSudahPadam.IsChecked = RbBukanKebakaran.IsChecked = false;
                PesanUi.Tampilkan(BarPesan, LblPesan, $"Verifikasi \"{Verifikasi.Label(status)}\" tersimpan.", JenisPesan.Info);
                await MuatVerifikasiAsync();
            }
            catch (ArgumentException ex)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan, ex.Message, JenisPesan.Peringatan);
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan,
                    "Verifikasi gagal disimpan: database tidak tersedia. " + PesanUi.Ringkas(ex.Message), JenisPesan.Error);
            }
            finally
            {
                BtnSimpanVerifikasi.IsEnabled = true;
            }
        }

        // ---- OPS-02: navigasi ke titik ----
        private void BtnSalinKoordinat_Click(object sender, RoutedEventArgs e)
        {
            if (_hotspot == null) return;
            try
            {
                Clipboard.SetText(KoordinatTeks(_hotspot));
                PesanUi.Tampilkan(BarPesan, LblPesan, $"Koordinat {KoordinatTeks(_hotspot)} disalin.", JenisPesan.Info);
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                // Clipboard bisa sedang dipakai aplikasi lain (COMException turunan ExternalException).
                PesanUi.Tampilkan(BarPesan, LblPesan, "Koordinat gagal disalin. Coba lagi.", JenisPesan.Peringatan);
            }
        }

        private void BtnBukaArah_Click(object sender, RoutedEventArgs e)
        {
            if (_hotspot == null) return;
            string url = "https://www.google.com/maps/dir/?api=1&destination=" +
                         Uri.EscapeDataString(KoordinatTeks(_hotspot).Replace(" ", ""));
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Win32Exception)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan,
                    "Browser tidak bisa dibuka. Salin koordinat lalu tempel di aplikasi peta.", JenisPesan.Peringatan);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_halamanKembali != null) Navigasi.Ke(this, _halamanKembali());
            else Navigasi.KeBeranda(this);
        }
    }
}
