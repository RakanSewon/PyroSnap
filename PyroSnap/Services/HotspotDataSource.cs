using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization; // Untuk parsing angka desimal (titik/koma)

namespace PyroSnap.Services
{
    public class HotspotDataSource
    {
        public string NamaSumber { get; private set; }
        public string EndpointUrl { get; private set; }
        public DateTime WaktuSinkronisasi { get; private set; }

        public List<Hotspot> Hotspots { get; private set; } = new List<Hotspot>();

        private static readonly HttpClient _client = new HttpClient();

        public HotspotDataSource(string nama, string url)
        {
            NamaSumber = nama;
            EndpointUrl = url;
        }

        public async Task<List<Hotspot>> FetchDataAsync()
        {
            try
            {
                // 1. Fetch data seperti requests.get() di Python
                HttpResponseMessage response = await _client.GetAsync(EndpointUrl);
                response.EnsureSuccessStatusCode();

                // 2. Baca respons sebagai teks mentah (format CSV)
                string csvData = await response.Content.ReadAsStringAsync();

                return ParseCsvToHotspots(csvData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Gagal fetch API {NamaSumber}: {ex.Message}");
                Debug.WriteLine("Menggunakan data dummy sebagai fallback...");
                return new List<Hotspot>
                {
                    new Hotspot(0.5071, 101.4478, 85, 332.4, 41.2, new DateTime(2026, 9, 12, 3, 0, 0))
                };
            }
        }

        // Fungsi manual pengganti pandas.read_csv() untuk MVP
        // Fungsi manual pengganti pandas.read_csv() untuk MVP
        private List<Hotspot> ParseCsvToHotspots(string csvData)
        {
            var hotspots = new List<Hotspot>();

            // Pecah teks menjadi array baris berdasarkan enter (\n)
            string[] baris = csvData.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Perulangan dimulai dari indeks 1 untuk melewati baris header kolom
            for (int i = 1; i < baris.Length; i++)
            {
                string[] kolom = baris[i].Split(',');

                // Pastikan jumlah kolom mencukupi sebelum memproses (mencegah error)
                if (kolom.Length >= 14)
                {
                    // Di C#, parsing angka butuh CultureInfo.InvariantCulture agar 
                    // tanda titik (.) pada latitude/longitude NASA dibaca sebagai desimal.
                    double lat = double.Parse(kolom[0], CultureInfo.InvariantCulture);
                    double lon = double.Parse(kolom[1], CultureInfo.InvariantCulture);

                    // Kolom ke-3 biasanya brightness/bright_ti4, kolom ke-10 confidence
                    double brightness = double.Parse(kolom[2], CultureInfo.InvariantCulture);

                    int confidence = 50;
                    if (int.TryParse(kolom[10], out int confVal))
                    {
                        confidence = confVal;
                    }

                    // --- TAMBAHKAN BARIS INI KEMBALI ---
                    // Kolom ke-13 di dataset MODIS/VIIRS umumnya adalah FRP (Fire Radiative Power)
                    double frp = double.Parse(kolom[12], CultureInfo.InvariantCulture);
                    // -----------------------------------

                    // Pemanggilan new Hotspot sekarang akan valid karena 'frp' sudah dideklarasikan
                    hotspots.Add(new Hotspot(lat, lon, confidence, brightness, frp, DateTime.Now));
                }
            }
            return hotspots;
        }

        public async Task<bool> SyncAsync()
        {
            List<Hotspot> data = await FetchDataAsync();
            if (data.Count == 0) return false;
            Hotspots = data;
            WaktuSinkronisasi = DateTime.Now;
            return true;
        }
    }
}