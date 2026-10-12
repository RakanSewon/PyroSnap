namespace PyroSnap;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks; // Wajib untuk async/Task
using System.Diagnostics;     // Untuk Debug.WriteLine

// Diagram: Hotspot --berasal dari--> HotspotDataSource
public class HotspotDataSource
{
    public string NamaSumber { get; private set; }
    public string EndpointUrl { get; private set; }
    public DateTime WaktuSinkronisasi { get; private set; }

    // Tambahkan HttpClient sebagai static agar efisien mengatur koneksi (seperti axios instance)
    private static readonly HttpClient _client = new HttpClient();

    public HotspotDataSource(string nama, string url)
    {
        NamaSumber = nama;
        EndpointUrl = url;
    }

    // Ubah nama menjadi FetchDataAsync dan kembalikan Task (mirip Promise di JS)
    public async Task<List<Hotspot>> FetchDataAsync()
    {
        try
        {
            // Coba ambil data dari API NASA sesungguhnya
            HttpResponseMessage response = await _client.GetAsync(EndpointUrl);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            List<Hotspot> hotspots = JsonSerializer.Deserialize<List<Hotspot>>(jsonResponse, options);

            return hotspots ?? new List<Hotspot>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Gagal fetch API {NamaSumber}: {ex.Message}");

            // SMART FALLBACK: Jika API error atau belum di-setup, 
            // kembalikan data contoh (dummy) buatanmu agar aplikasi tetap jalan
            Debug.WriteLine("Menggunakan data dummy (mock) sebagai fallback...");
            return new List<Hotspot>
            {
                new Hotspot(0.5071, 101.4478, 85, 332.4, 41.2, new DateTime(2026, 9, 12, 3, 0, 0)),
                new Hotspot(0.4433, 101.7621, 62, 318.9, 12.7, new DateTime(2026, 9, 12, 3, 42, 0)),
                new Hotspot(-0.9492, 102.1350, 95, 351.0, 128.6, new DateTime(2026, 9, 12, 5, 0, 0))
            };
        }
    }

    // Karena FetchData sekarang async, Sync juga harus menjadi async
    public async Task<bool> SyncAsync()
    {
        // Tunggu proses fetch selesai
        List<Hotspot> data = await FetchDataAsync();

        if (data.Count == 0)
        {
            return false;
        }

        WaktuSinkronisasi = DateTime.Now;
        return true;
    }
}