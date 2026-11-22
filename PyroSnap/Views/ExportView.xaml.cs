using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Npgsql;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// Ekspor laporan (use case Export Hotspot Report). Pratinjau dan file memakai teks
    /// yang sama dari HotspotReport.Generate(). Tidak tersedia untuk Tamu (K10).
    /// </summary>
    public partial class ExportView : Page
    {
        private const int BarisPratinjauMaks = 200;   // UX-10

        private readonly List<Hotspot> _data;

        public ExportView()
        {
            InitializeComponent();
            _data = AppSession.CurrentHotspots ?? new List<Hotspot>();
            Loaded += (_, _) =>
            {
                // SCR-A9 / RUI-05: penjaga hak ekspor.
                if (!Navigasi.PastikanHak(this, AppSession.BolehEkspor)) return;
                LblInfo.Text = _data.Count.ToString("N0");
                PerbaruiPreview();
            };
        }

        private bool IsCsv => RbCsv.IsChecked == true;

        private void Format_Changed(object sender, RoutedEventArgs e) => PerbaruiPreview();

        private HotspotReport BuatLaporan()
        {
            var report = new HotspotReport(IsCsv ? "CSV" : "TXT");
            report.MuatData(_data);
            report.Generate();
            return report;
        }

        private void PerbaruiPreview()
        {
            if (TxtPreview == null || LblPratinjauInfo == null) return;

            var report = BuatLaporan();
            if (report.Isi.Length == 0)
            {
                TxtPreview.Text = "(tidak ada data untuk dilaporkan)";
                LblPratinjauInfo.Text = "";
                return;
            }

            // Hanya sebagian baris yang ditampilkan supaya tetap ringan; file berisi semuanya.
            string[] baris = report.Isi.Split('\n');
            TxtPreview.Text = string.Join("\n", baris.Take(BarisPratinjauMaks));
            LblPratinjauInfo.Text = baris.Length > BarisPratinjauMaks
                ? $"Menampilkan {BarisPratinjauMaks} baris pertama dari {baris.Length}; file berisi semua baris."
                : $"{baris.Length} baris.";
        }

        private async void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (!AppSession.BolehEkspor) return;
            if (_data.Count == 0)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan,
                    "Tidak ada titik panas untuk diekspor. Longgarkan filter di Dashboard dulu.", JenisPesan.Peringatan);
                return;
            }

            string ext = IsCsv ? "csv" : "txt";
            var dlg = new SaveFileDialog
            {
                FileName = $"Laporan_Titik_Panas_{DateTime.Now:yyyyMMdd_HHmm}",
                DefaultExt = "." + ext,
                Filter = IsCsv ? "CSV (Excel)|*.csv" : "Teks|*.txt"
            };

            if (dlg.ShowDialog() != true) return;

            var report = BuatLaporan();
            try
            {
                report.ExportToFile(dlg.FileName);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show($"Laporan gagal disimpan:\n{ex.Message}\n\n" +
                    "Pastikan file tidak sedang dibuka aplikasi lain dan folder dapat ditulisi.",
                    "Ekspor Gagal", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BtnExport.IsEnabled = false;
            string catatan = await CatatKeDatabaseAsync(report, dlg.FileName);
            BtnExport.IsEnabled = true;

            MessageBox.Show($"Laporan berhasil diekspor ke:\n{dlg.FileName}\n\n{catatan}",
                "Ekspor Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// RPT-04/FT-05d: catat laporan + pembuatnya di database. File sudah tersimpan,
        /// jadi kegagalan di sini hanya diberitahukan, tidak membatalkan ekspor.
        /// </summary>
        private static async Task<string> CatatKeDatabaseAsync(HotspotReport report, string filePath)
        {
            if (AppSession.UserIdAngka is not int userId ||
                AppSession.NamaSumber == null || AppSession.EndpointTanpaKey == null)
            {
                return "Catatan: laporan tidak dicatat di database (sesi atau sumber data tidak diketahui).";
            }

            try
            {
                var (reportId, jumlah) = await ReportRepository.SimpanAsync(
                    report, userId, AppSession.NamaSumber, AppSession.EndpointTanpaKey, filePath);
                return $"Tercatat di database sebagai laporan #{reportId} ({jumlah} titik panas).";
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                return "Catatan: laporan tidak tercatat di database. " + PesanUi.Ringkas(ex.Message);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Navigasi.KeBeranda(this);
    }
}
