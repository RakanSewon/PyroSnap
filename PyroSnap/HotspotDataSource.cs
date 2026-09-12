namespace PyroSnap;

// Diagram: Hotspot --berasal dari--> HotspotDataSource
public class HotspotDataSource
{
    public string NamaSumber { get; private set; }
    public string EndpointUrl { get; private set; }
    public DateTime WaktuSinkronisasi { get; private set; }

    public HotspotDataSource(string nama, string url)
    {
        NamaSumber = nama;
        EndpointUrl = url;
    }

    public List<Hotspot> FetchData()
    {
        // TODO: sambungkan ke EndpointUrl (NASA FIRMS / BMKG).
        // Untuk sekarang dikembalikan data contoh supaya class lain bisa diuji.
        return new List<Hotspot>
        {
            new Hotspot(0.5071, 101.4478, 85, 332.4, 41.2, new DateTime(2026, 9, 12, 3, 0, 0)),
            new Hotspot(0.4433, 101.7621, 62, 318.9, 12.7, new DateTime(2026, 9, 12, 3, 42, 0)),
            new Hotspot(-0.9492, 102.1350, 95, 351.0, 128.6, new DateTime(2026, 9, 12, 5, 0, 0))
        };
    }

    public bool Sync()
    {
        List<Hotspot> data = FetchData();
        if (data.Count == 0)
        {
            return false;
        }

        WaktuSinkronisasi = DateTime.Now;
        return true;
    }
}
