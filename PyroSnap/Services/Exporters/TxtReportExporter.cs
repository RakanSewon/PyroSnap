using System;
using System.Collections.Generic;
using System.Text;

namespace PyroSnap.Services.Exporters
{
    /// <summary>
    /// Derived Class 1: Mewarisi ReportExporter (Inheritance) untuk format file TXT.
    /// Mengimplementasikan method GenerateContent secara polimorfik (Polymorphism - Method Overriding).
    /// </summary>
    public class TxtReportExporter : ReportExporter
    {
        /// <summary>
        /// Constructor default yang meneruskan konfigurasi ke constructor basis (base class constructor).
        /// </summary>
        public TxtReportExporter() : base("Laporan Pantauan Titik Panas", "TXT")
        {
        }

        /// <summary>
        /// Constructor dengan judul kustom yang juga meneruskan ke constructor basis.
        /// </summary>
        public TxtReportExporter(string judulKustom) : base(judulKustom, "TXT")
        {
        }

        /// <summary>
        /// Implementasi spesifik (override) pembentukan konten laporan teks.
        /// Memanfaatkan method protected ValidasiData() dan FormatHeader() dari kelas induk.
        /// </summary>
        public override string GenerateContent(List<Hotspot> data)
        {
            // Memanggil protected method dari base class untuk menjamin integritas data
            ValidasiData(data);

            var sb = new StringBuilder();
            // Memanggil protected virtual method dari base class
            sb.AppendLine(FormatHeader(data.Count));
            sb.AppendLine();
            sb.AppendLine("RINGKASAN TITIK PANAS (HOTSPOT DETAILS):");
            sb.AppendLine("--------------------------------------------------");

            for (int i = 0; i < data.Count; i++)
            {
                // Memanggil private helper method
                sb.AppendLine($"[{i + 1}] {FormatBarisTxt(data[i])}");
            }

            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine("Catatan: Data diperoleh dari satelit VIIRS/MODIS via PyroSnap.");

            return sb.ToString();
        }

        /// <summary>
        /// Helper method ber-access modifier private.
        /// Logika internal format baris teks hanya relevan dan dapat diakses di dalam kelas ini.
        /// </summary>
        private string FormatBarisTxt(Hotspot h)
        {
            return h.GetDetail();
        }
    }
}

