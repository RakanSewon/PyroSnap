using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace PyroSnap.Services
{
    /// <summary>
    /// Bagian bersama layanan web API pihak ketiga (Modul 10, pola Lab 10.1 dengan RestSharp):
    /// pembuatan RestClient, pengiriman + penggolongan kegagalan, cache memori, dan format angka.
    /// Kegagalan tidak pernah melempar exception ke UI; hasilnya HasilApi.Gagal dengan pesan singkat.
    /// </summary>
    internal static class LayananWeb
    {
        /// <summary>Identitas aplikasi (wajib untuk Nominatim). Jangan diganti User-Agent browser atau email pribadi.</summary>
        public const string UserAgent = "PyroSnap/1.0 (+https://github.com/RakanSewon/PyroSnap)";

        public static readonly TimeSpan BatasWaktu = TimeSpan.FromSeconds(10);

        public static RestClient BuatClient(string baseUrl) => new(new RestClientOptions(baseUrl)
        {
            Timeout = BatasWaktu,
            UserAgent = UserAgent
        });

        /// <summary>Angka untuk URL selalu memakai titik desimal (komputer berbahasa Indonesia memakai koma).</summary>
        public static string Angka(double nilai) => nilai.ToString("0.####", CultureInfo.InvariantCulture);

        /// <summary>Kunci cache: koordinat dibulatkan 2 desimal (±1 km).</summary>
        public static string KunciKoordinat(double lat, double lon) =>
            $"{Math.Round(lat, 2).ToString("0.00", CultureInfo.InvariantCulture)},{Math.Round(lon, 2).ToString("0.00", CultureInfo.InvariantCulture)}";

        /// <summary>
        /// Kirim permintaan dan golongkan kegagalan menjadi pesan bahasa awam.
        /// Hasil berhasil = isi respons (JSON mentah).
        /// </summary>
        public static async Task<HasilApi<string>> KirimAsync(RestClient client, RestRequest request, string namaLayanan,
                                                               CancellationToken ct = default)
        {
            RestResponse r;
            try
            {
                r = await client.ExecuteAsync(request, ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{namaLayanan}] {ex}");
                return HasilApi<string>.Gagal($"{namaLayanan} tidak bisa dihubungi.");
            }

            if (r.ResponseStatus == ResponseStatus.TimedOut)
                return HasilApi<string>.Gagal($"{namaLayanan} tidak menjawab dalam {BatasWaktu.TotalSeconds:0} detik.");

            if (r.StatusCode == 0)
            {
                Debug.WriteLine($"[{namaLayanan}] {r.ResponseStatus}: {r.ErrorMessage}");
                return HasilApi<string>.Gagal($"{namaLayanan} tidak bisa dihubungi. Periksa koneksi internet.");
            }

            if (r.StatusCode == (HttpStatusCode)429)
                return HasilApi<string>.Gagal($"{namaLayanan} sedang membatasi permintaan. Coba lagi beberapa saat lagi.");

            if (r.StatusCode == HttpStatusCode.Forbidden)
                return HasilApi<string>.Gagal($"{namaLayanan} menolak permintaan (HTTP 403).");

            if (!r.IsSuccessful)
            {
                // Open-Meteo mengirim {"error":true,"reason":"..."}; alasan teknis hanya untuk log.
                Debug.WriteLine($"[{namaLayanan}] HTTP {(int)r.StatusCode}: {r.Content}");
                return HasilApi<string>.Gagal($"{namaLayanan} menjawab dengan kesalahan (HTTP {(int)r.StatusCode}).");
            }

            if (string.IsNullOrWhiteSpace(r.Content))
                return HasilApi<string>.Gagal($"{namaLayanan} mengirim jawaban kosong.");

            return HasilApi<string>.Ok(r.Content);
        }

        // ---- Pembacaan JSON yang toleran (properti bisa tidak ada / null) ----
        public static double? AmbilAngka(JsonElement obj, string nama) =>
            obj.ValueKind == JsonValueKind.Object && obj.TryGetProperty(nama, out var v) && v.ValueKind == JsonValueKind.Number
                ? v.GetDouble() : null;

        public static string? AmbilTeks(JsonElement obj, string nama) =>
            obj.ValueKind == JsonValueKind.Object && obj.TryGetProperty(nama, out var v) && v.ValueKind == JsonValueKind.String
                ? v.GetString() : null;

        public static JsonElement AmbilObjek(JsonElement obj, string nama) =>
            obj.ValueKind == JsonValueKind.Object && obj.TryGetProperty(nama, out var v) && v.ValueKind == JsonValueKind.Object
                ? v : default;
    }

    /// <summary>Cache memori sederhana dengan masa berlaku (null = berlaku sampai aplikasi ditutup).</summary>
    internal sealed class CacheMemori<T> where T : class
    {
        private readonly Dictionary<string, (DateTime Disimpan, T Nilai)> _isi = new();
        private readonly TimeSpan? _berlaku;
        private readonly object _kunci = new();

        public CacheMemori(TimeSpan? berlaku) => _berlaku = berlaku;

        public T? Ambil(string kunci)
        {
            lock (_kunci)
            {
                if (!_isi.TryGetValue(kunci, out var e)) return null;
                if (_berlaku is TimeSpan b && DateTime.UtcNow - e.Disimpan > b)
                {
                    _isi.Remove(kunci);
                    return null;
                }
                return e.Nilai;
            }
        }

        public void Simpan(string kunci, T nilai)
        {
            lock (_kunci) _isi[kunci] = (DateTime.UtcNow, nilai);
        }

        public void Kosongkan()
        {
            lock (_kunci) _isi.Clear();
        }
    }
}
