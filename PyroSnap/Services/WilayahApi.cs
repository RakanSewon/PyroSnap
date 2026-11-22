using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace PyroSnap.Services
{
    /// <summary>
    /// WEB-04: nama wilayah administratif dari koordinat (Nominatim / OpenStreetMap, reverse geocoding).
    /// Kebijakan Nominatim (operations.osmfoundation.org/policies/nominatim): maksimal 1 permintaan/detik
    /// untuk seluruh aplikasi, User-Agent yang mengidentifikasi aplikasi, wajib cache, tanpa permintaan massal.
    /// Karena itu hanya dipanggil saat Detail dibuka, satu antrean, dan hasil disimpan sampai aplikasi ditutup.
    /// </summary>
    public static class WilayahApi
    {
        public const string NamaLayanan = "Layanan nama wilayah (OpenStreetMap)";
        public const string Atribusi = "Nama wilayah: © OpenStreetMap contributors (ODbL)";
        public const string Tautan = "https://www.openstreetmap.org/copyright";

        /// <summary>Jeda minimum antar-permintaan (kebijakan: 1/detik; diberi sedikit cadangan).</summary>
        public static readonly TimeSpan JedaMinimum = TimeSpan.FromMilliseconds(1100);

        /// <summary>Bila penyedia membatasi (429/403), berhenti meminta selama ini.</summary>
        public static readonly TimeSpan JedaSetelahDibatasi = TimeSpan.FromMinutes(10);

        private static readonly RestClient Client = LayananWeb.BuatClient("https://nominatim.openstreetmap.org/");
        private static readonly CacheMemori<NamaWilayah> Cache = new(null);
        private static readonly SemaphoreSlim Antrean = new(1, 1);
        private static DateTime _terakhirUtc = DateTime.MinValue;
        private static DateTime _dibatasiSampaiUtc = DateTime.MinValue;

        public static async Task<HasilApi<NamaWilayah>> AmbilAsync(double lat, double lon, CancellationToken ct = default)
        {
            string kunci = LayananWeb.KunciKoordinat(lat, lon);
            if (Cache.Ambil(kunci) is { } tersimpan) return HasilApi<NamaWilayah>.Ok(tersimpan);

            await Antrean.WaitAsync(ct);
            try
            {
                // Bisa saja sudah diisi permintaan lain selama menunggu antrean.
                if (Cache.Ambil(kunci) is { } baruDiisi) return HasilApi<NamaWilayah>.Ok(baruDiisi);

                if (DateTime.UtcNow < _dibatasiSampaiUtc)
                    return HasilApi<NamaWilayah>.Gagal($"{NamaLayanan} sedang membatasi permintaan. Coba lagi nanti.");

                TimeSpan sisa = JedaMinimum - (DateTime.UtcNow - _terakhirUtc);
                if (sisa > TimeSpan.Zero) await Task.Delay(sisa, ct);

                var request = new RestRequest("reverse")
                    .AddQueryParameter("lat", LayananWeb.Angka(lat))
                    .AddQueryParameter("lon", LayananWeb.Angka(lon))
                    .AddQueryParameter("format", "jsonv2")
                    .AddQueryParameter("zoom", "14")
                    .AddQueryParameter("addressdetails", "1")
                    .AddHeader("Accept-Language", "id");

                HasilApi<string> jawaban;
                try
                {
                    jawaban = await LayananWeb.KirimAsync(Client, request, NamaLayanan, ct);
                }
                finally
                {
                    _terakhirUtc = DateTime.UtcNow;
                }

                if (!jawaban.Berhasil)
                {
                    if (jawaban.Pesan.Contains("membatasi") || jawaban.Pesan.Contains("403"))
                        _dibatasiSampaiUtc = DateTime.UtcNow + JedaSetelahDibatasi;
                    return HasilApi<NamaWilayah>.Gagal(jawaban.Pesan);
                }

                var hasil = Baca(jawaban.Data!);
                if (hasil.Data is { } data) Cache.Simpan(kunci, data);
                return hasil;
            }
            finally
            {
                Antrean.Release();
            }
        }

        /// <summary>Baca JSON respons (format jsonv2). Terpisah agar bisa diuji tanpa internet.</summary>
        public static HasilApi<NamaWilayah> Baca(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return HasilApi<NamaWilayah>.Gagal($"{NamaLayanan} mengirim jawaban yang tidak dikenal.");

                // Tidak ada hasil: {"error":"Unable to geocode"}
                if (LayananWeb.AmbilTeks(root, "error") != null)
                    return HasilApi<NamaWilayah>.Gagal("Nama wilayah tidak ditemukan untuk titik ini.");

                var a = LayananWeb.AmbilObjek(root, "address");
                string? Pertama(params string[] kunci)
                {
                    foreach (var k in kunci)
                        if (LayananWeb.AmbilTeks(a, k) is { Length: > 0 } v) return v;
                    return null;
                }

                // Kunci alamat OSM berbeda per tempat; urutan dari yang paling umum di Indonesia.
                var data = new NamaWilayah
                {
                    Desa = Pertama("village", "suburb", "hamlet", "neighbourhood", "quarter"),
                    Kecamatan = Pertama("city_district", "district", "municipality"),
                    Kota = Pertama("city", "county", "town", "regency"),
                    Provinsi = Pertama("state", "province", "region"),
                    Lengkap = LayananWeb.AmbilTeks(root, "display_name") ?? ""
                };

                if (string.IsNullOrWhiteSpace(data.Ringkas))
                    return HasilApi<NamaWilayah>.Gagal("Nama wilayah tidak ditemukan untuk titik ini.");
                return HasilApi<NamaWilayah>.Ok(data);
            }
            catch (JsonException)
            {
                return HasilApi<NamaWilayah>.Gagal($"{NamaLayanan} mengirim jawaban yang tidak dikenal.");
            }
        }
    }
}
