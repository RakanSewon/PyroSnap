using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PyroSnap.Services.Exporters
{
    /// <summary>
    /// Derived Class 2: Mewarisi ReportExporter (Inheritance) untuk format file CSV tabular.
    /// Mengimplementasikan method GenerateContent secara polimorfik (Polymorphism - Method Overriding).
    /// </summary>
    public class CsvReportExporter : ReportExporter
    {
        /// <summary>
        /// Constructor default yang memanggil constructor basis (base class constructor).
        /// </summary>
        public CsvReportExporter() : base("Data Tabular Titik Panas", "CSV")
        {
        }

        /// <summary>
        /// Constructor dengan judul kustom yang memanggil constructor basis.
        /// </summary>
        public CsvReportExporter(string judulKustom) : base(judulKustom, "CSV")
        {
        }

        /// <summary>
        /// Implementasi spesifik (override) pembentukan konten CSV baris dan kolom.
        /// Menggunakan InvariantCulture agar separator desimal konsisten (titik bukan koma).
        /// </summary>
        public override string GenerateContent(List<Hotspot> data)
        {
            // Memanggil protected method dari base class
            ValidasiData(data);

            var sb = new StringBuilder();

            // Header kolom CSV
            sb.AppendLine("No,Latitude,Longitude,Confidence,Brightness_K,FRP_MW,WaktuAkuisisi");

            for (int i = 0; i < data.Count; i++)
            {
                var h = data[i];
                // Memanggil private helper method untuk menyusun baris CSV
                sb.AppendLine(FormatBarisCsv(i + 1, h));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Helper method ber-access modifier private.
        /// Logika formatting nilai numerik dan tanggal ke format kolom CSV disembunyikan di dalam kelas ini.
        /// </summary>
        private string FormatBarisCsv(int nomor, Hotspot h)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1:F4},{2:F4},{3},{4:F1},{5:F1},{6:yyyy-MM-dd HH:mm}",
                nomor,
                h.Latitude,
                h.Longitude,
                h.Confidence,
                h.Brightness,
                h.Frp,
                h.WaktuAkuisisi
            );
        }
    }
}
