using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// WEB-05: kartu "Kondisi sekitar" di Detail — nama wilayah (Nominatim), kualitas udara dan
    /// angin/cuaca (Open-Meteo). Ketiganya dipanggil bersamaan; masing-masing bisa gagal sendiri
    /// dan hanya menampilkan "tidak tersedia" tanpa mengganggu bagian lain (WEB-08).
    /// </summary>
    public partial class DetailView
    {
        private bool _memuatKondisi;

        private async Task MuatKondisiAsync(Hotspot h)
        {
            if (_memuatKondisi) return;
            _memuatKondisi = true;
            BtnMuatKondisi.IsEnabled = false;

            LblWilayahAdmin.Text = "Memuat...";
            LblUdara.Text = "Memuat...";
            LblUdaraSaran.Text = "";
            ChipUdara.Visibility = Visibility.Collapsed;
            LblAngin.Text = "Memuat...";
            LblArahAngin.Text = "";
            LblCuacaLain.Text = "";
            LblKondisiWaktu.Text = "";

            try
            {
                var udara = KualitasUdaraApi.AmbilAsync(h.Latitude, h.Longitude);
                var cuaca = CuacaApi.AmbilAsync(h.Latitude, h.Longitude);
                var wilayah = WilayahApi.AmbilAsync(h.Latitude, h.Longitude);

                TampilkanUdara(await udara);
                TampilkanCuaca(await cuaca);
                TampilkanWilayah(await wilayah);
            }
            catch (Exception ex)
            {
                // Pengaman terakhir: layanan sudah menangkap kegagalan jaringan sendiri.
                Debug.WriteLine($"[Kondisi sekitar] {ex}");
                LblKondisiWaktu.Text = "Kondisi sekitar tidak bisa dimuat.";
            }
            finally
            {
                _memuatKondisi = false;
                BtnMuatKondisi.IsEnabled = true;
            }
        }

        private void TampilkanUdara(HasilApi<KualitasUdara> hasil)
        {
            if (hasil.Data is not { } u)
            {
                LblUdara.Text = "Tidak tersedia";
                LblUdaraSaran.Text = hasil.Pesan;
                return;
            }

            LblUdara.Text = u.Pm25 is double pm ? $"PM2.5 {pm:0.#} µg/m³" : "PM2.5 tidak tersedia";
            if (u.UsAqi is int aqi)
            {
                var k = KualitasUdaraApi.Kategori(aqi);
                LblUdaraKategori.Text = $"{k.Nama} · AQI {aqi}";
                LblUdaraSaran.Text = k.Saran;
                (string bg, string fg) = k.Tingkat switch
                {
                    0 => ("DeltaNaikBg", "DeltaNaikFg"),
                    1 => ("PesanPeringatanBg", "PesanPeringatanFg"),
                    _ => ("DeltaTurunBg", "DeltaTurunFg")
                };
                ChipUdara.SetResourceReference(BackgroundProperty, bg);
                LblUdaraKategori.SetResourceReference(ForegroundProperty, fg);
                ChipUdara.Visibility = Visibility.Visible;
            }
            if (u.Pm10 is double pm10) LblUdaraSaran.Text = $"PM10 {pm10:0.#} µg/m³. " + LblUdaraSaran.Text;
            TulisWaktu(u.Waktu, u.Zona);
        }

        private void TampilkanCuaca(HasilApi<KondisiCuaca> hasil)
        {
            if (hasil.Data is not { } c)
            {
                LblAngin.Text = "Tidak tersedia";
                LblArahAngin.Text = hasil.Pesan;
                return;
            }

            LblAngin.Text = c.AnginKmj is double v ? $"Angin {v:0.#} km/jam" : "Kecepatan angin tidak tersedia";
            LblArahAngin.Text = CuacaApi.TeksArah(c);

            var lain = new System.Collections.Generic.List<string>();
            if (c.Kelembapan is double rh) lain.Add($"Kelembapan {rh:0}%");
            if (c.Suhu is double t) lain.Add($"Suhu {t:0.#} °C");
            if (c.Hujan is double p) lain.Add(p > 0 ? $"Hujan {p:0.#} mm" : "Tidak hujan");
            LblCuacaLain.Text = string.Join(" · ", lain);
            TulisWaktu(c.Waktu, c.Zona);
        }

        private void TampilkanWilayah(HasilApi<NamaWilayah> hasil)
        {
            if (hasil.Data is not { } w)
            {
                LblWilayahAdmin.Text = "Tidak tersedia — " + hasil.Pesan;
                LblWilayahAdmin.ToolTip = null;
                return;
            }

            LblWilayahAdmin.Text = w.Ringkas;
            LblWilayahAdmin.ToolTip = w.Lengkap;
        }

        /// <summary>Tulis waktu data (waktu setempat titik). Dipanggil dua layanan; yang terakhir menang.</summary>
        private void TulisWaktu(string waktuIso, string zona)
        {
            if (!DateTime.TryParse(waktuIso, System.Globalization.CultureInfo.InvariantCulture,
                                   System.Globalization.DateTimeStyles.None, out var w)) return;
            LblKondisiWaktu.Text = $"Data pukul {w:HH:mm} waktu setempat{(zona.Length > 0 ? $" ({zona})" : "")}.";
        }

        private async void BtnMuatKondisi_Click(object sender, RoutedEventArgs e)
        {
            if (_hotspot != null) await MuatKondisiAsync(_hotspot);
        }

        private void Tautan_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan, "Browser tidak bisa dibuka. Alamat: " + e.Uri.AbsoluteUri, JenisPesan.Peringatan);
            }
            e.Handled = true;
        }
    }
}
