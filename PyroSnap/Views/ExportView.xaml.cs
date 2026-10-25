using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace PyroSnap.Views
{
    /// <summary>
    /// Tampilan Export Laporan (use case Export Hotspot Report).
    /// Memakai HotspotReport milik tim untuk format TXT; CSV dibangun di layer UI.
    /// </summary>
    public partial class ExportView : Page
    {
        private readonly List<Hotspot> _data;

        public ExportView()
        {
            InitializeComponent();
            _data = AppSession.CurrentHotspots ?? new List<Hotspot>();
            LblInfo.Text = $"Jumlah hotspot: {_data.Count}";
            PerbaruiPreview();
        }

        private bool IsCsv => RbCsv.IsChecked == true;

        private void Format_Changed(object sender, RoutedEventArgs e)
        {
            PerbaruiPreview();
        }

        private void PerbaruiPreview()
        {
            if (TxtPreview == null) return;
            TxtPreview.Text = IsCsv ? BuatCsv() : BuatTxt();
        }

        /// <summary>Format TXT memanfaatkan HotspotReport.Generate() milik tim.</summary>
        private string BuatTxt()
        {
            var report = new HotspotReport("TXT");
            report.MuatData(_data);
            if (!report.Generate())
                return "(tidak ada data untuk dilaporkan)";

            // HotspotReport menyimpan isi secara privat; bangun ulang preview setara
            // dengan memanggil GetDetail() seperti yang dilakukan Generate().
            var sb = new StringBuilder();
            sb.AppendLine("LAPORAN HOTSPOT (TXT)");
            sb.AppendLine("Jumlah titik panas: " + _data.Count);
            foreach (var h in _data)
                sb.AppendLine(h.GetDetail());
            return sb.ToString();
        }

        private string BuatCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Latitude,Longitude,Confidence,Brightness,FRP,WaktuAkuisisi");
            foreach (var h in _data)
            {
                sb.AppendLine($"{h.Latitude},{h.Longitude},{h.Confidence},{h.Brightness},{h.Frp}," +
                              $"{h.WaktuAkuisisi:yyyy-MM-dd HH:mm}");
            }
            if (_data.Count == 0) sb.AppendLine("(tidak ada data)");
            return sb.ToString();
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_data.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk diekspor.", "PyroSnap");
                return;
            }

            string ext = IsCsv ? "csv" : "txt";
            var dlg = new SaveFileDialog
            {
                FileName = "Laporan_Hotspot_PyroSnap",
                DefaultExt = "." + ext,
                Filter = IsCsv ? "CSV (Excel)|*.csv" : "Teks|*.txt"
            };

            if (dlg.ShowDialog() != true) return;

            if (IsCsv)
            {
                File.WriteAllText(dlg.FileName, BuatCsv());
            }
            else
            {
                // Pakai alur HotspotReport milik tim untuk file TXT.
                var report = new HotspotReport("TXT");
                report.MuatData(_data);
                report.Generate();
                report.ExportToFile(dlg.FileName);
            }

            MessageBox.Show($"Laporan berhasil diekspor ke:\n{dlg.FileName}",
                "Export Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Window.GetWindow(this)).Navigate(new DashboardView());
        }
    }
}
