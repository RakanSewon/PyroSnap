using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PyroSnap.Services.Exporters
{
    /// <summary>
    /// Derived Class 3: Mewarisi ReportExporter (Inheritance) untuk format terstruktur JSON.
    /// Mengimplementasikan method GenerateContent secara polimorfik (Polymorphism - Method Overriding).
    /// </summary>
    public class JsonReportExporter : ReportExporter
    {
        /// <summary>
        /// Constructor default yang memanggil constructor basis (base class constructor).
        /// </summary>
        public JsonReportExporter() : base("Data Terstruktur Titik Panas", "JSON")
        {
        }

        /// <summary>
        /// Constructor dengan judul kustom yang memanggil constructor basis.
        /// </summary>
        public JsonReportExporter(string judulKustom) : base(judulKustom, "JSON")
        {
        }

        /// <summary>
        /// Implementasi spesifik (override) pembentukan konten dokumen berformat JSON.
        /// Mengubah koleksi objek Hotspot ke dalam struktur JSON terindentasi rapi.
        /// </summary>
        public override string GenerateContent(List<Hotspot> data)
        {
            // Memanggil protected method base class untuk menjamin integritas data
            ValidasiData(data);

            var reportPayload = new
            {
                judul = JudulLaporan,
                format = Format,
                tanggal_dibuat = TanggalDibuat.ToString("yyyy-MM-dd HH:mm:ss"),
                jumlah_titik = data.Count,
                sumber_data = "NASA FIRMS Satellite (VIIRS/MODIS)",
                hotspots = UbahKeFormatJson(data)
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };

            return JsonSerializer.Serialize(reportPayload, options);
        }

        /// <summary>
        /// Helper method ber-access modifier private.
        /// Mengonversi List Hotspot ke format struktur list objek yang bersih untuk JSON serialization.
        /// </summary>
        private List<object> UbahKeFormatJson(List<Hotspot> data)
        {
            var list = new List<object>();
            for (int i = 0; i < data.Count; i++)
            {
                var h = data[i];
                list.Add(new
                {
                    nomor = i + 1,
                    latitude = h.Latitude,
                    longitude = h.Longitude,
                    confidence_persen = h.Confidence,
                    tingkat_keparahan = h.Confidence >= 80 ? "Tinggi" : h.Confidence >= 50 ? "Sedang" : "Rendah",
                    brightness_kelvin = h.Brightness,
                    frp_mw = h.Frp,
                    waktu_akuisisi = h.WaktuAkuisisi.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            return list;
        }
    }
}

