using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace PyroSnap.Services
{
    /// <summary>
    /// WEB-02: kualitas udara saat ini dari Open-Meteo Air Quality API (data CAMS), tanpa key.
    /// Lisensi CC BY 4.0 — tampilan wajib mencantumkan Atribusi. Gratis untuk non-komersial.
    /// Pola kelas statis seperti Ongkir.cs di Lab 10.1.
    /// </summary>
    public static class KualitasUdaraApi
    {
        public const string NamaLayanan = "Layanan kualitas udara (Open-Meteo)";
        public const string Atribusi = "Data kualitas udara: Open-Meteo.com (CAMS), CC BY 4.0";
        public const string Tautan = "https://open-meteo.com/";

        private static readonly RestClient Client = LayananWeb.BuatClient("https://air-quality-api.open-meteo.com/");
        private static readonly CacheMemori<KualitasUdara> Cache = new(TimeSpan.FromHours(1));

        public static async Task<HasilApi<KualitasUdara>> AmbilAsync(double lat, double lon, CancellationToken ct = default)
        {
            string kunci = LayananWeb.KunciKoordinat(lat, lon);
            if (Cache.Ambil(kunci) is { } tersimpan) return HasilApi<KualitasUdara>.Ok(tersimpan);

            var request = new RestRequest("v1/air-quality")
                .AddQueryParameter("latitude", LayananWeb.Angka(lat))
                .AddQueryParameter("longitude", LayananWeb.Angka(lon))
                .AddQueryParameter("current", "pm2_5,pm10,us_aqi")
                .AddQueryParameter("timezone", "auto");

            var jawaban = await LayananWeb.KirimAsync(Client, request, NamaLayanan, ct);
            if (!jawaban.Berhasil) return HasilApi<KualitasUdara>.Gagal(jawaban.Pesan);

            var hasil = Baca(jawaban.Data!);
            if (hasil.Data is { } data) Cache.Simpan(kunci, data);
            return hasil;
        }

        /// <summary>Baca JSON respons. Terpisah agar bisa diuji tanpa internet.</summary>
        public static HasilApi<KualitasUdara> Baca(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var cur = LayananWeb.AmbilObjek(root, "current");
                if (cur.ValueKind != JsonValueKind.Object)
                    return HasilApi<KualitasUdara>.Gagal($"{NamaLayanan} mengirim jawaban yang tidak dikenal.");

                double? aqi = LayananWeb.AmbilAngka(cur, "us_aqi");
                var data = new KualitasUdara
                {
                    Pm25 = LayananWeb.AmbilAngka(cur, "pm2_5"),
                    Pm10 = LayananWeb.AmbilAngka(cur, "pm10"),
                    UsAqi = aqi is double a ? (int)Math.Round(a) : null,
                    Waktu = LayananWeb.AmbilTeks(cur, "time") ?? "",
                    Zona = LayananWeb.AmbilTeks(root, "timezone_abbreviation") ?? ""
                };

                if (data.Pm25 == null && data.UsAqi == null)
                    return HasilApi<KualitasUdara>.Gagal("Data kualitas udara belum tersedia untuk lokasi ini.");
                return HasilApi<KualitasUdara>.Ok(data);
            }
            catch (JsonException)
            {
                return HasilApi<KualitasUdara>.Gagal($"{NamaLayanan} mengirim jawaban yang tidak dikenal.");
            }
        }

        /// <summary>
        /// Kategori US AQI (EPA) dalam bahasa Indonesia + saran singkat.
        /// Kategori ISPU (KLHK) belum dipakai karena batas angkanya belum diverifikasi (web-api.md §7).
        /// </summary>
        public static (string Nama, string Saran, int Tingkat) Kategori(int usAqi) => usAqi switch
        {
            <= 50 => ("Baik", "Aman untuk beraktivitas di luar.", 0),
            <= 100 => ("Sedang", "Orang yang sangat sensitif sebaiknya mengurangi aktivitas berat di luar.", 1),
            <= 150 => ("Tidak sehat bagi kelompok sensitif", "Anak, lansia, ibu hamil, dan penderita penyakit pernapasan kurangi aktivitas di luar.", 2),
            <= 200 => ("Tidak sehat", "Kurangi aktivitas di luar; pakai masker yang sesuai untuk asap.", 3),
            <= 300 => ("Sangat tidak sehat", "Hindari aktivitas di luar; tutup jendela.", 4),
            _ => ("Berbahaya", "Tetap di dalam ruangan dan ikuti arahan petugas.", 5)
        };
    }
}
