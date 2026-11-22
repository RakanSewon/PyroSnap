using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Npgsql;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    // Bagian Dashboard: wilayah saya + banner peringatan warga (PUB-01) dan ringkasan per wilayah (PUB-02).
    public partial class DashboardView
    {
        // ---- PUB-01: wilayah saya + banner peringatan warga (Publik/Tamu) ----
        private async Task MuatWilayahPilihanAsync()
        {
            if (!AppSession.IsWarga) return;

            LblWilayahSayaInfo.Text = AppSession.WilayahTersimpan
                ? "Tersimpan di akun Anda."
                : "Sebagai tamu, pilihan hanya berlaku sampai Anda keluar.";

            if (AppSession.WilayahTersimpan && !AppSession.WilayahPilihanDimuat && AppSession.UserIdAngka is int userId)
            {
                try
                {
                    AppSession.WilayahPilihan = await PreferensiRepository.AmbilWilayahPilihanAsync(userId);
                    AppSession.WilayahPilihanDimuat = true;
                }
                catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
                {
                    LblWilayahSayaInfo.Text = "Wilayah tersimpan tidak bisa dibaca: database tidak tersedia.";
                }
            }

            CmbWilayahSaya.SelectedItem = WilayahPantau.CariPersis(AppSession.WilayahPilihan)?.NamaLokasi;
            PerbaruiBannerWarga();
        }

        private async void BtnSimpanWilayah_Click(object sender, RoutedEventArgs e)
        {
            if (!AppSession.IsWarga) return;
            if (CmbWilayahSaya.SelectedItem is not string wilayah)
            {
                LblWilayahSayaInfo.Text = "Pilih wilayah dari daftar dulu.";
                return;
            }

            AppSession.WilayahPilihan = wilayah;
            PerbaruiBannerWarga();

            if (!AppSession.WilayahTersimpan || AppSession.UserIdAngka is not int userId)
            {
                LblWilayahSayaInfo.Text = $"Wilayah {wilayah} dipakai untuk sesi tamu ini.";
                return;
            }

            BtnSimpanWilayah.IsEnabled = false;
            try
            {
                await PreferensiRepository.SimpanWilayahPilihanAsync(userId, wilayah);
                AppSession.WilayahPilihanDimuat = true;
                LblWilayahSayaInfo.Text = $"Wilayah {wilayah} tersimpan di akun Anda.";
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                LblWilayahSayaInfo.Text = "Wilayah dipakai untuk sesi ini, tetapi gagal disimpan: database tidak tersedia.";
            }
            finally
            {
                BtnSimpanWilayah.IsEnabled = true;
            }
        }

        /// <summary>
        /// Peringatan berbahasa awam untuk wilayah pilihan (copy-and-accessibility.md §3).
        /// Memakai data terbaru (bukan historis) dan tidak terpengaruh filter.
        /// </summary>
        private void PerbaruiBannerWarga()
        {
            if (!AppSession.IsWarga)
            {
                BarWarga.Visibility = Visibility.Collapsed;
                return;
            }

            var area = WilayahPantau.CariPersis(AppSession.WilayahPilihan);
            _ = PerbaruiUdaraWargaAsync(area);
            if (area == null)
            {
                PesanUi.Tampilkan(BarWarga, LblWarga,
                    "Pilih wilayah Anda di bagian \"Wilayah saya\" (kolom kanan) untuk melihat peringatan titik panas dan kualitas udara di sekitar Anda.",
                    JenisPesan.Info);
                return;
            }

            var diArea = AppSession.SemuaHotspot.Where(h => WilayahPantau.DalamArea(h, area)).ToList();
            var tinggi = diArea.Where(h => TingkatHotspot.Dari(h.Confidence) == Tingkat.Tinggi).ToList();

            if (tinggi.Count > 0)
            {
                DateTime terbaru = tinggi.Max(h => h.WaktuAkuisisi).ToLocalTime();
                PesanUi.Tampilkan(BarWarga, LblWarga,
                    $"Waspada: ada {tinggi.Count} titik panas berkeyakinan tinggi terdeteksi di sekitar {area.NamaLokasi} " +
                    $"(terbaru {terbaru:dd MMM HH:mm}). Hindari membakar lahan atau sampah, kurangi aktivitas di luar " +
                    "bila ada asap, dan ikuti informasi resmi setempat. Lihat Panduan & Kontak Darurat.",
                    JenisPesan.Peringatan);
            }
            else if (diArea.Count > 0)
            {
                PesanUi.Tampilkan(BarWarga, LblWarga,
                    $"Ada {diArea.Count} titik panas berkeyakinan rendah/nominal di sekitar {area.NamaLokasi}. " +
                    "Belum tentu kebakaran; tetap waspada dan pantau informasi resmi.",
                    JenisPesan.Info);
            }
            else
            {
                PesanUi.Tampilkan(BarWarga, LblWarga,
                    $"Tidak ada titik panas terdeteksi di sekitar {area.NamaLokasi} dalam data terbaru.",
                    JenisPesan.Info);
            }
        }

        // ---- WEB-06: kualitas udara di pusat wilayah pilihan (Open-Meteo, data CAMS) ----
        private string? _udaraDiminta;

        private async Task PerbaruiUdaraWargaAsync(Location? area)
        {
            if (area == null)
            {
                PanelUdaraWarga.Visibility = Visibility.Collapsed;
                _udaraDiminta = null;
                return;
            }

            // Hindari permintaan ganda untuk wilayah yang sama (banner diperbarui berkali-kali).
            if (_udaraDiminta == area.NamaLokasi) return;
            _udaraDiminta = area.NamaLokasi;

            PanelUdaraWarga.Visibility = Visibility.Visible;
            LblUdaraWargaJudul.Text = $"Kualitas udara di {area.NamaLokasi}";
            LblUdaraWarga.Text = "Memuat...";
            LblUdaraWargaSaran.Text = "";
            ChipUdaraWarga.Visibility = Visibility.Collapsed;

            var hasil = await KualitasUdaraApi.AmbilAsync(area.Latitude, area.Longitude);
            if (_udaraDiminta != area.NamaLokasi) return;   // wilayah sudah diganti selama menunggu

            if (hasil.Data is not { } u)
            {
                LblUdaraWarga.Text = "Tidak tersedia";
                LblUdaraWargaSaran.Text = hasil.Pesan;
                _udaraDiminta = null;   // boleh dicoba lagi pada pembaruan berikutnya
                return;
            }

            LblUdaraWarga.Text = u.Pm25 is double pm ? $"PM2.5 {pm:0.#} µg/m³" : "PM2.5 tidak tersedia";
            if (u.UsAqi is int aqi)
            {
                var k = KualitasUdaraApi.Kategori(aqi);
                LblUdaraWargaKategori.Text = k.Nama;
                LblUdaraWargaSaran.Text = $"Indeks {aqi}. {k.Saran} Diukur di pusat area pantau, bukan di rumah Anda.";
                (string bg, string fg) = k.Tingkat switch
                {
                    0 => ("DeltaNaikBg", "DeltaNaikFg"),
                    1 => ("PesanPeringatanBg", "PesanPeringatanFg"),
                    _ => ("DeltaTurunBg", "DeltaTurunFg")
                };
                ChipUdaraWarga.SetResourceReference(BackgroundProperty, bg);
                LblUdaraWargaKategori.SetResourceReference(ForegroundProperty, fg);
                ChipUdaraWarga.Visibility = Visibility.Visible;
            }
        }

        private void Tautan_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
            {
                TampilkanStatus("Browser tidak bisa dibuka.", e.Uri.AbsoluteUri, JenisPesan.Peringatan);
            }
            e.Handled = true;
        }

        // ---- PUB-02: ringkasan per wilayah dari data yang sedang dipakai ----
        private void PerbaruiRingkasan()
        {
            GridRingkasan.ItemsSource = _semua
                .GroupBy(h => WilayahPantau.Temukan(h)?.NamaLokasi ?? "Di luar area pantau")
                .Select(g => new RingkasanRow(g.Key, g.ToList()))
                .OrderByDescending(r => r.Tinggi)
                .ThenByDescending(r => r.Jumlah)
                .ToList();
        }
    }

    /// <summary>Satu baris ringkasan risiko per wilayah.</summary>
    public class RingkasanRow
    {
        public RingkasanRow(string wilayah, List<Hotspot> data)
        {
            Wilayah = wilayah;
            Jumlah = data.Count;
            Tinggi = data.Count(h => TingkatHotspot.Dari(h.Confidence) == Views.Tingkat.Tinggi);
            Nominal = data.Count(h => TingkatHotspot.Dari(h.Confidence) == Views.Tingkat.Nominal);
            Rendah = data.Count(h => TingkatHotspot.Dari(h.Confidence) == Views.Tingkat.Rendah);
            FrpMaks = data.Count == 0 ? 0 : data.Max(h => h.Frp);
        }

        public string Wilayah { get; }
        public int Jumlah { get; }
        public int Tinggi { get; }
        public int Nominal { get; }
        public int Rendah { get; }
        public double FrpMaks { get; }
        public string FrpMaksText => $"{FrpMaks:0.##} MW";

        /// <summary>Status singkat untuk warga.</summary>
        public string Status => Tinggi > 0 ? "Waspada" : Jumlah > 0 ? "Pantau" : "Aman";
    }
}
