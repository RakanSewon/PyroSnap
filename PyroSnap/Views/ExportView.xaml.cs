using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PyroSnap.Services.Exporters;

namespace PyroSnap.Views
{
    /// <summary>
    /// Tampilan Export Laporan (use case: Export Hotspot Report).
    /// Mengimplementasikan konsep Polymorphism dari arsitektur ReportExporter
    /// untuk mendukung berbagai format laporan (TXT, CSV, JSON) secara dinamis.
    /// </summary>
    public partial class ExportView : Page
    {
        private readonly List<Hotspot> _data;

        public ExportView()
        {
            InitializeComponent();
            _data = AppSession.CurrentHotspots ?? new List<Hotspot>();
            LblInfo.Text = $"{_data.Count} Titik Panas Aktif";
            if (_data.Count > 0)
            {
                double avgConf = 0;
                foreach (var h in _data) avgConf += h.Confidence;
                avgConf /= _data.Count;
                LblDetailRingkas.Text = $"Rata-rata Confidence: {avgConf:F1}% | Sumber: NASA FIRMS";
            }
            PerbaruiPreview();
        }

        /// <summary>
        /// Factory helper method untuk menghasilkan objek turunan ReportExporter.
        /// Objek kelas turunan (TxtReportExporter, CsvReportExporter, atau JsonReportExporter)
        /// diperlakukan sebagai tipe base class (ReportExporter) yang mewujudkan konsep Polymorphism.
        /// </summary>
        private ReportExporter DapatkanExporter()
        {
            if (RbJson?.IsChecked == true)
                return new JsonReportExporter();
            if (RbCsv?.IsChecked == true)
                return new CsvReportExporter();
            return new TxtReportExporter();
        }

        private void Format_Changed(object sender, RoutedEventArgs e)
        {
            PerbaruiPreview();
        }

        /// <summary>
        /// Memperbarui pratinjau teks laporan dengan pemanggilan polimorfik method GenerateContent().
        /// </summary>
        private void PerbaruiPreview()
        {
            if (TxtPreview == null) return;

            if (_data.Count == 0)
            {
                TxtPreview.Text = "(tidak ada data titik panas untuk dilaporkan)";
                if (LblStatusPreview != null) LblStatusPreview.Text = "Tidak ada data";
                if (LblUkuranPreview != null) LblUkuranPreview.Text = "0 karakter";
                return;
            }

            try
            {
                // Polymorphism: Pemanggilan GenerateContent() dieksekusi sesuai kelas konkretnya
                ReportExporter exporter = DapatkanExporter();
                string konten = exporter.GenerateContent(_data);
                TxtPreview.Text = konten;

                if (LblBadgeFormat != null)
                    LblBadgeFormat.Text = exporter.Format;

                if (LblStatusPreview != null)
                    LblStatusPreview.Text = $"Format {exporter.Format} aktif";

                if (LblUkuranPreview != null)
                    LblUkuranPreview.Text = $"{konten.Length:N0} karakter ({konten.Split('\n').Length:N0} baris)";
            }
            catch (Exception ex)
            {
                TxtPreview.Text = $"Perhatian: {ex.Message}";
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_data.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk diekspor.", "PyroSnap");
                return;
            }

            // Penerapan Polymorphism: Objek bertipe abstract base class (ReportExporter)
            ReportExporter exporter = DapatkanExporter();
            string ext = exporter.Format.ToLower();

            string filterDesc = ext switch
            {
                "json" => "JSON File (*.json)|*.json",
                "csv"  => "CSV (Excel) (*.csv)|*.csv",
                _      => "Teks Dokumen (*.txt)|*.txt"
            };

            var dlg = new SaveFileDialog
            {
                FileName = $"Laporan_Hotspot_PyroSnap_{DateTime.Now:yyyyMMdd_HHmm}",
                DefaultExt = "." + ext,
                Filter = filterDesc
            };

            if (dlg.ShowDialog() != true) return;

            // Penerapan Method Overloading dengan parameter 'out' (Lab 9.1)
            bool sukses = exporter.ExportToFile(_data, dlg.FileName, out string statusPesan);

            if (sukses)
            {
                MessageBox.Show($"{statusPesan}\n\nLokasi penyimpanan file:\n{dlg.FileName}",
                    "Export Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(statusPesan, "Export Gagal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtPreview?.Text)) return;
            Clipboard.SetText(TxtPreview.Text);
            MessageBox.Show("Isi pratinjau berhasil disalin ke clipboard!", "PyroSnap",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Window.GetWindow(this)).Navigate(new DashboardView());
        }
    }
}
