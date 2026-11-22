using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    // Bagian Dashboard: memuat data (NASA FIRMS / database / historis), status, dan peringatan.
    public partial class DashboardView
    {
        // ---- Use case: Sinkronisasi / muat data ----
        // Admin            : ambil dari NASA FIRMS lalu simpan ke database; bila FIRMS gagal,
        //                    tampilkan data terakhir dari database.
        // Operator/Publik/Tamu: tidak boleh sinkronisasi, jadi membaca dari database.
        private async Task MuatDataAsync()
        {
            if (_sedangMemuat) return;
            _sedangMemuat = true;
            BtnReload.IsEnabled = false;
            LoadingOverlay.Visibility = Visibility.Visible;

            try
            {
                if (AppSession.BolehSinkron)
                    await SinkronDariFirmsAsync();
                else
                    await MuatDariDatabaseAsync(alasanFirmsGagal: null);
            }
            finally
            {
                _sedangMemuat = false;
                BtnReload.IsEnabled = true;
            }
        }

        private async Task SinkronDariFirmsAsync()
        {
            string produk = AppConfig.FirmsSource;
            LblMapStatus.Text = "Memuat data dari NASA FIRMS...";
            TampilkanStatus("Memuat data titik panas dari NASA FIRMS...", "", JenisPesan.Info);

            string? key = AppConfig.FirmsMapKey;
            if (key == null)
            {
                await MuatDariDatabaseAsync($"Key NASA FIRMS belum diatur ({AppConfig.EnvFirmsKey} di file .env).");
                return;
            }

            var source = HotspotDataSource.NasaFirms(key, produk);
            if (!await source.SyncAsync())
            {
                await MuatDariDatabaseAsync(source.PesanGagal ?? "Penyebab tidak diketahui.");
                return;
            }

            string endpoint = HotspotDataSource.FirmsUrlTanpaKey(produk);
            string utama = $"{source.Hotspots.Count} titik panas · {source.NamaSumber} · " +
                           $"diperbarui {source.WaktuSinkronisasi:dd MMM yyyy HH:mm}";
            var rincian = new List<string>();
            if (source.JumlahBarisDilewati > 0)
                rincian.Add($"{source.JumlahBarisDilewati} baris data rusak dilewati");
            var jenis = JenisPesan.Info;

            // FT-08a / BE-11: simpan ke database. Gagal simpan tidak membatalkan tampilan data.
            bool tersimpan = false;
            try
            {
                int baru = await HotspotRepository.SimpanHasilSinkronAsync(
                    source.NamaSumber, endpoint, source.Hotspots, source.WaktuSinkronisasi);
                rincian.Add($"{baru} titik baru disimpan ke database");
                tersimpan = true;
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                rincian.Add("gagal menyimpan ke database: " + PesanUi.Ringkas(ex.Message));
                jenis = JenisPesan.Peringatan;
            }

            // FT-07 / ALR-04: hotspot di atas ambang kritis memicu peringatan operasional
            // (Admin & Operator). Butuh hotspot yang sudah tersimpan.
            if (tersimpan)
            {
                var kritis = AmbangKritis.BuatAlert(source.Hotspots);
                if (kritis.Count > 0)
                {
                    try
                    {
                        int alertBaru = await AlertRepository.KirimKeSemuaUserAsync(source.NamaSumber, kritis);
                        rincian.Add($"{kritis.Count} titik kritis (≥ {AmbangKritis.ConfidenceMin}%)" +
                                    (alertBaru > 0 ? ", peringatan dikirim ke Admin & Operator" : ", peringatan sudah pernah dikirim"));
                    }
                    catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
                    {
                        rincian.Add("gagal membuat peringatan: " + PesanUi.Ringkas(ex.Message));
                        jenis = JenisPesan.Peringatan;
                    }
                }
            }

            TerimaData(source.Hotspots, source.NamaSumber, endpoint, utama, string.Join(" · ", rincian), jenis);
        }

        /// <param name="alasanFirmsGagal">null bila memang membaca dari database;
        /// berisi pesan bila pengambilan dari NASA FIRMS gagal.</param>
        private async Task MuatDariDatabaseAsync(string? alasanFirmsGagal)
        {
            string produk = AppConfig.FirmsSource;
            string nama = HotspotDataSource.NamaFirms(produk);
            LblMapStatus.Text = "Memuat data dari database...";
            if (alasanFirmsGagal == null)
                TampilkanStatus("Memuat data titik panas...", "", JenisPesan.Info);

            List<Hotspot> data;
            DateTime? waktuSinkron;
            try
            {
                (data, waktuSinkron) = await HotspotRepository.MuatTerbaruAsync(nama);
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                TampilkanGagalMuat(GabungAlasan(alasanFirmsGagal, "Database tidak tersedia: " + PesanUi.Ringkas(ex.Message)));
                return;
            }

            if (data.Count == 0)
            {
                TampilkanGagalMuat(GabungAlasan(alasanFirmsGagal, AppSession.BolehSinkron
                    ? "Belum ada data di database."
                    : "Belum ada data. Data diperbarui oleh petugas; coba lagi nanti."));
                return;
            }

            string waktu = waktuSinkron?.ToLocalTime().ToString("dd MMM yyyy HH:mm") ?? "tidak diketahui";
            string utama = $"{data.Count} titik panas · diperbarui {waktu}";
            string rincian = alasanFirmsGagal == null
                ? $"Sumber: {nama} (data tersimpan)"
                : $"Gagal memuat dari NASA FIRMS: {alasanFirmsGagal} Menampilkan data terakhir dari database.";
            TerimaData(data, nama, HotspotDataSource.FirmsUrlTanpaKey(produk), utama, rincian,
                       alasanFirmsGagal == null ? JenisPesan.Info : JenisPesan.Peringatan);
        }

        private static string GabungAlasan(string? alasanFirms, string pesan) =>
            alasanFirms == null ? pesan : $"Gagal memuat dari NASA FIRMS: {alasanFirms} {pesan}";

        private void TerimaData(List<Hotspot> data, string namaSumber, string endpointTanpaKey,
                                string utama, string rincian, JenisPesan jenis)
        {
            AppSession.SemuaHotspot = data;
            AppSession.NamaSumber = namaSumber;
            AppSession.EndpointTanpaKey = endpointTanpaKey;
            AppSession.DataDimuat = true;
            AppSession.StatusUtama = utama;
            AppSession.StatusDetail = rincian;
            AppSession.StatusJenis = jenis;
            AppSession.RentangHistoris = null;

            _semua = data;
            _fitPeta = true;
            LoadingOverlay.Visibility = Visibility.Collapsed;
            BtnDataTerbaru.IsEnabled = false;
            TampilkanStatus(utama, rincian, jenis);
            TerapkanKriteria();
            PerbaruiBannerWarga();
        }

        /// <summary>
        /// Kegagalan ditampilkan apa adanya; tidak ada data contoh pengganti.
        /// Bila ada data sebelumnya, data itu tetap dipakai dan diberi keterangan.
        /// </summary>
        private void TampilkanGagalMuat(string pesan)
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;

            if (_semua.Count > 0 && AppSession.DataDimuat)
            {
                TampilkanStatus("Gagal memuat ulang. Data sebelumnya tetap ditampilkan.", pesan, JenisPesan.Error);
                return;
            }

            // Belum ada data sama sekali (UX-08: pesan jelas di bilah status, bukan dialog).
            string saran = AppSession.BolehSinkron ? " Tekan \"Perbarui Data dari NASA FIRMS\" untuk mencoba lagi." : "";
            TampilkanStatus("Data titik panas belum bisa dimuat.", pesan + saran, JenisPesan.Error);
            TerapkanKriteria(); // daftar & peta kosong
            PerbaruiBannerWarga();
        }

        private void PulihkanDariSesi()
        {
            _semua = AppSession.RentangHistoris != null ? AppSession.DataHistoris : AppSession.SemuaHotspot;
            _filter = AppSession.Filter;
            _areaFokus = AppSession.AreaFokus;
            _fitPeta = _areaFokus == null;

            CmbLokasi.Text = AppSession.TeksCari;
            SldConfidence.Value = _filter.ConfidenceMin;
            SldFrp.Value = _filter.FrpMin;

            if (AppSession.RentangHistoris is { } r)
            {
                DpDari.SelectedDate = r.Dari;
                DpSampai.SelectedDate = r.Sampai;
                BtnDataTerbaru.IsEnabled = true;
                TampilkanStatus($"Data historis {r.Dari:dd MMM yyyy} – {r.Sampai:dd MMM yyyy} · {_semua.Count} titik panas",
                                "Sumber: database", JenisPesan.Info);
            }
            else
            {
                BtnDataTerbaru.IsEnabled = false;
                TampilkanStatus(AppSession.StatusUtama, AppSession.StatusDetail, AppSession.StatusJenis);
            }

            TerapkanKriteria(AppSession.SelectedHotspot);
            PerbaruiBannerWarga();
        }

        private void TampilkanStatus(string utama, string rincian, JenisPesan jenis)
        {
            PesanUi.Tampilkan(BarStatus, LblStatusUtama, utama, jenis);
            LblStatusDetail.Text = rincian;
            LblStatusDetail.Visibility = string.IsNullOrEmpty(rincian) ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void BtnReload_Click(object sender, RoutedEventArgs e)
        {
            if (!AppSession.BolehSinkron) return;

            try
            {
                await MuatDataAsync();
                await PerbaruiJumlahAlertAsync();
            }
            catch (Exception ex)
            {
                TampilkanStatus("Terjadi kesalahan saat memperbarui data.", PesanUi.Ringkas(ex.Message), JenisPesan.Error);
            }
        }

        // ---- Peringatan operasional (FT-07c / ALR-06), hanya Admin & Operator ----
        /// <summary>Tampilkan jumlah peringatan belum dibaca milik user di tombol header.</summary>
        private async Task PerbaruiJumlahAlertAsync()
        {
            if (!AppSession.TerimaAlertOperasional || AppSession.UserIdAngka is not int userId) return;

            try
            {
                int belumDibaca = await AlertRepository.JumlahBelumDibacaAsync(userId);
                BtnAlert.Content = belumDibaca > 0 ? $"🔔 Peringatan ({belumDibaca})" : "🔔 Peringatan";
                BtnAlert.ToolTip = belumDibaca > 0 ? $"{belumDibaca} peringatan belum dibaca" : "Tidak ada peringatan baru";
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                BtnAlert.Content = "🔔 Peringatan (?)";
                BtnAlert.ToolTip = "Jumlah peringatan tidak bisa dibaca: database tidak tersedia.";
            }
        }
    }
}
