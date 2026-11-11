using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PyroSnap.Services.Exporters
{
    /// <summary>
    /// Abstract Base Class untuk sistem ekspor laporan (Pewarisan/Inheritance & Abstraction).
    /// Mengimplementasikan enkapsulasi data, batasan hak akses (access modifier),
    /// serta method virtual dan abstract untuk mendukung polymorphism.
    /// </summary>
    public abstract class ReportExporter
    {
        // Enkapsulasi: private backing field
        private string _judulLaporan = string.Empty;

        /// <summary>
        /// Judul laporan yang di-enkapsulasi dengan validasi pada setter.
        /// Melempar ArgumentException bila bernilai kosong atau whitespace.
        /// </summary>
        public string JudulLaporan
        {
            get => _judulLaporan;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Judul laporan tidak boleh kosong.", nameof(value));
                _judulLaporan = value;
            }
        }

        /// <summary>
        /// Format keluaran laporan (TXT, CSV, dsb).
        /// Getter bersifat public, setter bersifat protected (hanya kelas ini dan turunannya yang boleh mengubah).
        /// </summary>
        public string Format { get; protected set; }

        /// <summary>
        /// Waktu pembuatan laporan.
        /// Getter bersifat public, setter bersifat protected untuk melindungi integritas audit log.
        /// </summary>
        public DateTime TanggalDibuat { get; protected set; }

        /// <summary>
        /// Constructor basis untuk menginisialisasi judul dan format laporan.
        /// Dipanggil oleh kelas turunan menggunakan sintaks ': base(...)'.
        /// </summary>
        public ReportExporter(string judulLaporan, string format)
        {
            JudulLaporan = judulLaporan;
            Format = format;
            TanggalDibuat = DateTime.Now;
        }

        /// <summary>
        /// Method abstract untuk menyusun konten teks laporan.
        /// Wajib di-override oleh kelas turunan sesuai format spesifik masing-masing (Polymorphism).
        /// Access modifier: public.
        /// </summary>
        public abstract string GenerateContent(List<Hotspot> data);

        /// <summary>
        /// Template method untuk menulis isi laporan ke file fisik pada disk.
        /// Access modifier: public virtual (dapat di-override jika subclass butuh penanganan khusus).
        /// </summary>
        public virtual void ExportToFile(List<Hotspot> data, string filePath)
        {
            string content = GenerateContent(data);
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        /// <summary>
        /// Method Overloading dari ExportToFile dengan output parameter (out parameter).
        /// Mengembalikan boolean status keberhasilan serta pesan ringkasan ekspor.
        /// Access modifier: public virtual.
        /// </summary>
        public virtual bool ExportToFile(List<Hotspot> data, string filePath, out string statusMessage)
        {
            try
            {
                ExportToFile(data, filePath);
                var fileInfo = new FileInfo(filePath);
                statusMessage = $"Laporan ({Format}) berhasil diekspor: {data.Count} titik ({fileInfo.Length} bytes).";
                return true;
            }
            catch (Exception ex)
            {
                statusMessage = $"Gagal mengekspor laporan: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Helper method ber-access modifier protected untuk memvalidasi integritas data sebelum diekspor.
        /// Hanya dapat diakses oleh kelas ini dan derived class-nya.
        /// </summary>
        protected void ValidasiData(List<Hotspot> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data hotspot tidak boleh bernilai null.");

            if (data.Count == 0)
                throw new InvalidOperationException("Tidak ada data titik panas untuk diproses ke dalam laporan.");
        }

        /// <summary>
        /// Helper method ber-access modifier protected virtual untuk menyusun header laporan.
        /// Dapat digunakan langsung oleh kelas turunan atau di-override sesuai kebutuhan format.
        /// </summary>
        protected virtual string FormatHeader(int jumlahTitik)
        {
            var sb = new StringBuilder();
            sb.AppendLine("==================================================");
            sb.AppendLine($"PYROSNAP - {JudulLaporan.ToUpper()}");
            sb.AppendLine($"Format Laporan : {Format}");
            sb.AppendLine($"Tanggal Cetak  : {TanggalDibuat:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Jumlah Titik   : {jumlahTitik}");
            sb.AppendLine("==================================================");
            return sb.ToString();
        }
    }
}

