namespace PyroSnap;

// Modul 10 (layanan web API pihak ketiga, di luar class diagram): data tambahan di sekitar
// sebuah titik dari Open-Meteo dan Nominatim. Lihat docs/backend/web-api.md.

/// <summary>Hasil panggilan layanan web: data, atau pesan singkat untuk pengguna bila gagal.</summary>
public sealed class HasilApi<T> where T : class
{
    public T? Data { get; }
    public string Pesan { get; }
    public bool Berhasil => Data != null;

    private HasilApi(T? data, string pesan)
    {
        Data = data;
        Pesan = pesan;
    }

    public static HasilApi<T> Ok(T data) => new(data, "");
    public static HasilApi<T> Gagal(string pesan) => new(null, pesan);
}

/// <summary>Kualitas udara saat ini (Open-Meteo Air Quality, data CAMS).</summary>
public sealed class KualitasUdara
{
    public double? Pm25 { get; init; }      // µg/m³
    public double? Pm10 { get; init; }      // µg/m³
    public int? UsAqi { get; init; }        // indeks kualitas udara AS (EPA)
    public string Waktu { get; init; } = "";  // waktu setempat, mis. "2026-11-23T01:00"
    public string Zona { get; init; } = "";   // mis. "GMT+7"
}

/// <summary>Cuaca saat ini (Open-Meteo Forecast).</summary>
public sealed class KondisiCuaca
{
    public double? AnginKmj { get; init; }        // kecepatan angin 10 m, km/jam
    public double? ArahAnginDerajat { get; init; } // arah ASAL angin (meteorologi), 0 = dari utara
    public double? Kelembapan { get; init; }      // %
    public double? Suhu { get; init; }            // °C
    public double? Hujan { get; init; }           // mm
    public string Waktu { get; init; } = "";
    public string Zona { get; init; } = "";
}

/// <summary>Nama wilayah administratif dari koordinat (Nominatim / OpenStreetMap).</summary>
public sealed class NamaWilayah
{
    public string? Desa { get; init; }       // desa/kelurahan
    public string? Kecamatan { get; init; }
    public string? Kota { get; init; }       // kota/kabupaten
    public string? Provinsi { get; init; }
    public string Lengkap { get; init; } = "";  // display_name dari Nominatim

    /// <summary>Nama ringkas: desa/kelurahan, kota/kabupaten, provinsi (yang tersedia saja).</summary>
    public string Ringkas
    {
        get
        {
            var bagian = new[] { Desa, Kecamatan, Kota, Provinsi }
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Distinct()
                .ToList();
            return bagian.Count > 0 ? string.Join(", ", bagian) : Lengkap;
        }
    }
}
