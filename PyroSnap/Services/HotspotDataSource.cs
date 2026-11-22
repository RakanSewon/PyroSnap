using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PyroSnap.Services
{
    public class HotspotDataSource
    {
        // {0} = MAP_KEY, {1} = produk/satelit (mis. VIIRS_NOAA20_NRT). Bbox Indonesia, 1 hari.
        private const string FirmsUrlTemplate =
            "https://firms.modaps.eosdis.nasa.gov/api/area/csv/{0}/{1}/95,-11,141,6/1";

        public string NamaSumber { get; private set; }
        public string EndpointUrl { get; private set; }
        public DateTime WaktuSinkronisasi { get; private set; }

        public List<Hotspot> Hotspots { get; private set; } = new List<Hotspot>();

        /// <summary>Pesan kegagalan sinkronisasi terakhir; null bila sukses.</summary>
        public string? PesanGagal { get; private set; }

        /// <summary>Jumlah baris CSV yang dilewati karena rusak pada sinkronisasi terakhir.</summary>
        public int JumlahBarisDilewati { get; private set; }

        private static readonly HttpClient _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public HotspotDataSource(string nama, string url)
        {
            NamaSumber = nama;
            EndpointUrl = url;
        }

        /// <summary>Sumber NASA FIRMS untuk wilayah Indonesia dengan key dan produk dari konfigurasi.</summary>
        public static HotspotDataSource NasaFirms(string mapKey, string produk)
        {
            string url = string.Format(FirmsUrlTemplate,
                Uri.EscapeDataString(mapKey), Uri.EscapeDataString(produk));
            return new HotspotDataSource(NamaFirms(produk), url);
        }

        /// <summary>Nama sumber FIRMS; dipakai juga sebagai kunci baris hotspot_data_source.</summary>
        public static string NamaFirms(string produk) => $"NASA FIRMS ({produk})";

        /// <summary>URL FIRMS dengan penanda {KEY}; aman disimpan di database.</summary>
        public static string FirmsUrlTanpaKey(string produk) =>
            string.Format(FirmsUrlTemplate, "{KEY}", Uri.EscapeDataString(produk));

        /// <summary>
        /// Ambil dan parse data. Melempar exception berpesan jelas bila gagal;
        /// tidak pernah mengembalikan data contoh.
        /// </summary>
        public async Task<List<Hotspot>> FetchDataAsync(CancellationToken ct = default)
        {
            string csvData;
            try
            {
                using HttpResponseMessage response = await _client.GetAsync(EndpointUrl, ct);
                csvData = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException(PesanStatus(response.StatusCode), null, response.StatusCode);
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"{NamaSumber} tidak merespons dalam {_client.Timeout.TotalSeconds:0} detik.");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == null)
            {
                throw new HttpRequestException(
                    $"Tidak dapat terhubung ke {NamaSumber}. Periksa koneksi internet.", ex);
            }

            List<Hotspot> hasil = ParseCsv(csvData, out int dilewati);
            JumlahBarisDilewati = dilewati;
            return hasil;
        }

        /// <summary>
        /// true bila data berhasil diambil (boleh 0 titik). Bila gagal: false,
        /// <see cref="PesanGagal"/> terisi, dan data sebelumnya dipertahankan.
        /// </summary>
        public async Task<bool> SyncAsync(CancellationToken ct = default)
        {
            try
            {
                Hotspots = await FetchDataAsync(ct);
                WaktuSinkronisasi = DateTime.Now;
                PesanGagal = null;
                return true;
            }
            catch (Exception ex) when (ex is HttpRequestException or TimeoutException or InvalidDataException)
            {
                PesanGagal = ex.Message;
                return false;
            }
        }

        private static string PesanStatus(HttpStatusCode status) => status switch
        {
            HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                $"Permintaan ditolak NASA FIRMS ({(int)status}). Periksa FIRMS_MAP_KEY di .env.",
            HttpStatusCode.TooManyRequests =>
                "Kuota NASA FIRMS habis untuk sementara (429). Coba lagi beberapa menit lagi.",
            _ => $"NASA FIRMS membalas dengan status {(int)status}."
        };

        /// <summary>
        /// Parse CSV FIRMS. Kolom dicari berdasarkan nama header, bukan indeks tetap.
        /// Baris rusak dilewati dan dihitung di <paramref name="dilewati"/>.
        /// </summary>
        internal static List<Hotspot> ParseCsv(string csvData, out int dilewati)
        {
            var hotspots = new List<Hotspot>();
            dilewati = 0;

            string[] baris = csvData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (baris.Length == 0)
                throw new InvalidDataException("Respons NASA FIRMS kosong.");

            string[] header = baris[0].Trim().Split(',');
            int iLat = Array.IndexOf(header, "latitude");
            int iLon = Array.IndexOf(header, "longitude");
            int iBright = Array.IndexOf(header, "bright_ti4");                    // VIIRS
            if (iBright < 0) iBright = Array.IndexOf(header, "brightness");     // MODIS
            int iDate = Array.IndexOf(header, "acq_date");
            int iTime = Array.IndexOf(header, "acq_time");
            int iConf = Array.IndexOf(header, "confidence");
            int iFrp = Array.IndexOf(header, "frp");

            if (iLat < 0 || iLon < 0 || iBright < 0 || iDate < 0 || iTime < 0 || iConf < 0 || iFrp < 0)
            {
                // Mis. key salah: FIRMS bisa membalas teks biasa, bukan CSV.
                string awal = baris[0].Length > 80 ? baris[0][..80] + "…" : baris[0];
                throw new InvalidDataException($"Format data NASA FIRMS tidak dikenali: \"{awal}\"");
            }

            for (int i = 1; i < baris.Length; i++)
            {
                string[] kolom = baris[i].Trim().Split(',');
                if (kolom.Length < header.Length ||
                    !TryParseDouble(kolom[iLat], out double lat) ||
                    !TryParseDouble(kolom[iLon], out double lon) ||
                    !TryParseDouble(kolom[iBright], out double brightness) ||
                    !TryParseDouble(kolom[iFrp], out double frp) ||
                    !TryParseConfidence(kolom[iConf], out int confidence) ||
                    !TryParseWaktuUtc(kolom[iDate], kolom[iTime], out DateTime waktu))
                {
                    dilewati++;
                    continue;
                }

                hotspots.Add(new Hotspot(lat, lon, confidence, brightness, frp, waktu));
            }

            return hotspots;
        }

        private static bool TryParseDouble(string s, out double nilai) =>
            double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out nilai);

        /// <summary>
        /// VIIRS memberi kategori l/n/h; MODIS memberi angka 0–100.
        /// Pemetaan kategori (keputusan SCH-04 opsi A, diperbarui 2026-11-22): l=20, n=60, h=90,
        /// sehingga masuk kelas LAPAN yang benar: Rendah (0–29), Nominal (30–79), Tinggi (80–100).
        /// </summary>
        internal static bool TryParseConfidence(string s, out int confidence)
        {
            switch (s.Trim().ToLowerInvariant())
            {
                case "l": confidence = 20; return true;
                case "n": confidence = 60; return true;
                case "h": confidence = 90; return true;
            }
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out confidence)
                   && confidence >= 0 && confidence <= 100;
        }

        /// <summary>acq_date (yyyy-MM-dd) + acq_time (HHmm UTC, tanpa nol di depan, mis. "338").</summary>
        internal static bool TryParseWaktuUtc(string tanggal, string jam, out DateTime waktu)
        {
            waktu = default;
            string hhmm = jam.Trim().PadLeft(4, '0');
            if (hhmm.Length != 4) return false;

            return DateTime.TryParseExact(
                tanggal.Trim() + " " + hhmm, "yyyy-MM-dd HHmm", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out waktu);
        }
    }
}
