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
    /// untuk mendukung berbagai format laporan (TXT, CSV) secara dinamis.
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

        /// <summary>
        /// Factory helper method untuk menghasilkan objek turunan ReportExporter.
        /// Objek kelas turunan (TxtReportExporter atau CsvReportExporter) diperlakukan
        /// sebagai tipe base class (ReportExporter) yang mewujudkan konsep Polymorphism.
        /// </summary>
        private ReportExporter DapatkanExporter()
        {
            return IsCsv ? new CsvReportExporter() : new TxtReportExporter();
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
                TxtPreview.Text = "(tidak ada data untuk dilaporkan)";
                return;
            }

            try
            {
                // Polymorphism: Pemanggilan GenerateContent() dieksekusi sesuai kelas konkretnya
                ReportExporter exporter = DapatkanExporter();
                TxtPreview.Text = exporter.GenerateContent(_data);
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

            string ext = IsCsv ? "csv" : "txt";
            var dlg = new SaveFileDialog
            {
                FileName = $"Laporan_Hotspot_PyroSnap_{DateTime.Now:yyyyMMdd_HHmm}",
                DefaultExt = "." + ext,
                Filter = IsCsv ? "CSV (Excel)|*.csv" : "Teks|*.txt"
            };

            if (dlg.ShowDialog() != true) return;

            // Penerapan Polymorphism: Objek bertipe abstract base class (ReportExporter)
            ReportExporter exporter = DapatkanExporter();

            // Penerapan Method Overloading dengan parameter 'out' (Lab 9.1)
            bool sukses = exporter.ExportToFile(_data, dlg.FileName, out string statusPesan);

            if (sukses)
            {
                MessageBox.Show($"{statusPesan}\n\nLokasi file:\n{dlg.FileName}",
                    "Export Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(statusPesan, "Export Gagal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Window.GetWindow(this)).Navigate(new DashboardView());
        }
    }
}
