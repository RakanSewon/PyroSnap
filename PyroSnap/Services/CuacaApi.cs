using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace PyroSnap.Services
{
    /// <summary>
    /// WEB-03: angin, kelembapan, suhu, dan hujan saat ini dari Open-Meteo Forecast API, tanpa key.
    /// Arah angin dipakai untuk memperkirakan arah sebaran asap dari titik panas.
    /// Lisensi CC BY 4.0 — tampilan wajib mencantumkan Atribusi.
    /// </summary>
    public static class CuacaApi
    {
        public const string NamaLayanan = "Layanan cuaca (Open-Meteo)";
        public const string Atribusi = "Data cuaca: Open-Meteo.com, CC BY 4.0";
        public const string Tautan = "https://open-meteo.com/";

        /// <summary>Di bawah kecepatan ini angin dianggap tenang (arah tidak berarti).</summary>
        public const double BatasTenangKmj = 2;

        private static readonly RestClient Client = LayananWeb.BuatClient("https://api.open-meteo.com/");
        private static readonly CacheMemori<KondisiCuaca> Cache = new(TimeSpan.FromHours(1));

        private static readonly string[] MataAngin =
            { "Utara", "Timur Laut", "Timur", "Tenggara", "Selatan", "Barat Daya", "Barat", "Barat Laut" };

        public static async Task<HasilApi<KondisiCuaca>> AmbilAsync(double lat, double lon, CancellationToken ct = default)
        {
            string kunci = LayananWeb.KunciKoordinat(lat, lon);
            if (Cache.Ambil(kunci) is { } tersimpan) return HasilApi<KondisiCuaca>.Ok(tersimpan);

            var request = new RestRequest("v1/forecast")
                .AddQueryParameter("latitude", LayananWeb.Angka(lat))
                .AddQueryParameter("longitude", LayananWeb.Angka(lon))
                .AddQueryParameter("current", "wind_speed_10m,wind_direction_10m,relative_humidity_2m,temperature_2m,precipitation")
                .AddQueryParameter("wind_speed_unit", "kmh")
                .AddQueryParameter("timezone", "auto");

            var jawaban = await LayananWeb.KirimAsync(Client, request, NamaLayanan, ct);
            if (!jawaban.Berhasil) return HasilApi<KondisiCuaca>.Gagal(jawaban.Pesan);

            var hasil = Baca(jawaban.Data!);
            if (hasil.Data is { } data) Cache.Simpan(kunci, data);
            return hasil;
        }

        /// <summary>Baca JSON respons. Terpisah agar bisa diuji tanpa internet.</summary>
        public static HasilApi<KondisiCuaca> Baca(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var cur = LayananWeb.AmbilObjek(root, "current");
                if (cur.ValueKind != JsonValueKind.Object)
                    return HasilApi<KondisiCuaca>.Gagal($"{NamaLayanan} mengirim jawaban yang tidak dikenal.");

                var data = new KondisiCuaca
                {
                    AnginKmj = LayananWeb.AmbilAngka(cur, "wind_speed_10m"),
                    ArahAnginDerajat = LayananWeb.AmbilAngka(cur, "wind_direction_10m"),
                    Kelembapan = LayananWeb.AmbilAngka(cur, "relative_humidity_2m"),
                    Suhu = LayananWeb.AmbilAngka(cur, "temperature_2m"),
                    Hujan = LayananWeb.AmbilAngka(cur, "precipitation"),
                    Waktu = LayananWeb.AmbilTeks(cur, "time") ?? "",
                    Zona = LayananWeb.AmbilTeks(root, "timezone_abbreviation") ?? ""
                };

                if (data.AnginKmj == null && data.Kelembapan == null && data.Suhu == null)
                    return HasilApi<KondisiCuaca>.Gagal("Data cuaca belum tersedia untuk lokasi ini.");
                return HasilApi<KondisiCuaca>.Ok(data);
            }
            catch (JsonException)
            {
                return HasilApi<KondisiCuaca>.Gagal($"{NamaLayanan} mengirim jawaban yang tidak dikenal.");
            }
        }

        /// <summary>Derajat (0 = utara, searah jarum jam) ke 8 arah mata angin.</summary>
        public static string ArahMataAngin(double derajat)
        {
            double d = ((derajat % 360) + 360) % 360;
            return MataAngin[(int)Math.Round(d / 45.0) % 8];
        }

        /// <summary>
        /// Teks arah angin + perkiraan arah asap. wind_direction_10m adalah arah ASAL angin,
        /// jadi asap bergerak ke arah sebaliknya (+180°).
        /// </summary>
        public static string TeksArah(KondisiCuaca c)
        {
            if (c.ArahAnginDerajat is not double arah) return "Arah angin tidak tersedia";
            if (c.AnginKmj is double v && v < BatasTenangKmj) return "Angin tenang; asap cenderung tidak menyebar jauh";
            return $"Angin dari {ArahMataAngin(arah)} ({arah:0}°) · asap diperkirakan bergerak ke {ArahMataAngin(arah + 180)}";
        }
    }
}
