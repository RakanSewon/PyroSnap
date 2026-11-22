using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    // Bagian Dashboard: data historis per rentang tanggal (OPS-03, Admin & Operator).
    public partial class DashboardView
    {
        // ---- OPS-03: data historis (Admin & Operator) ----
        private const int MaksHariHistoris = 31;

        private async void BtnHistoris_Click(object sender, RoutedEventArgs e)
        {
            if (!AppSession.BolehHistoris) return;

            if (DpDari.SelectedDate is not DateTime dari || DpSampai.SelectedDate is not DateTime sampai)
            {
                TampilkanStatus("Pilih tanggal awal dan akhir dulu.", "", JenisPesan.Peringatan);
                return;
            }
            if (sampai < dari)
            {
                TampilkanStatus("Tanggal akhir tidak boleh sebelum tanggal awal.", "", JenisPesan.Peringatan);
                return;
            }
            if ((sampai - dari).TotalDays + 1 > MaksHariHistoris)
            {
                TampilkanStatus($"Rentang maksimal {MaksHariHistoris} hari.", "", JenisPesan.Peringatan);
                return;
            }

            string nama = AppSession.NamaSumber ?? HotspotDataSource.NamaFirms(AppConfig.FirmsSource);
            var filter = new FilterCriteria
            {
                // Tanggal dipilih dalam waktu lokal; seluruh hari "sampai" ikut.
                WaktuMulai = DateTime.SpecifyKind(dari.Date, DateTimeKind.Local).ToUniversalTime(),
                WaktuAkhir = DateTime.SpecifyKind(sampai.Date.AddDays(1).AddTicks(-1), DateTimeKind.Local).ToUniversalTime()
            };

            try
            {
                TampilkanStatus("Memuat data historis dari database...", "", JenisPesan.Info);
                List<Hotspot> data = await HotspotRepository.QueryAsync(nama, filter);

                AppSession.RentangHistoris = (dari.Date, sampai.Date);
                AppSession.DataHistoris = data;
                _semua = data;
                _fitPeta = true;
                BtnDataTerbaru.IsEnabled = true;

                TampilkanStatus($"Data historis {dari:dd MMM yyyy} – {sampai:dd MMM yyyy} · {data.Count} titik panas",
                                data.Count == 0 ? "Tidak ada titik panas tersimpan pada rentang ini." : "Sumber: database",
                                JenisPesan.Info);
                TerapkanKriteria();
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                TampilkanStatus("Data historis tidak bisa dimuat.", "Database tidak tersedia: " + PesanUi.Ringkas(ex.Message),
                                JenisPesan.Error);
            }
        }

        private void BtnDataTerbaru_Click(object sender, RoutedEventArgs e)
        {
            AppSession.RentangHistoris = null;
            AppSession.DataHistoris = new List<Hotspot>();
            _semua = AppSession.SemuaHotspot;
            _fitPeta = true;
            BtnDataTerbaru.IsEnabled = false;
            TampilkanStatus(AppSession.StatusUtama, AppSession.StatusDetail, AppSession.StatusJenis);
            TerapkanKriteria();
        }
    }
}
