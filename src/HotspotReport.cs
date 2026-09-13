using System.Text;

namespace PyroSnap;

// Diagram: HotspotReport --berisi--> Hotspot (agregasi)
public class HotspotReport
{
    private string isi = "";

    public string Format { get; private set; }
    public DateTime TanggalDibuat { get; private set; }
    public List<Hotspot> DaftarHotspot { get; private set; }

    public HotspotReport(string format)
    {
        Format = format;
        DaftarHotspot = new List<Hotspot>();
    }

    public bool Generate()
    {
        if (DaftarHotspot.Count == 0)
        {
            return false;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("LAPORAN HOTSPOT (" + Format + ")");
        sb.AppendLine("Jumlah titik panas: " + DaftarHotspot.Count);

        foreach (Hotspot h in DaftarHotspot)
        {
            sb.AppendLine(h.GetDetail());
        }

        isi = sb.ToString();
        TanggalDibuat = DateTime.Now;
        return true;
    }

    public void ExportToFile(string path)
    {
        File.WriteAllText(path, isi);
    }
}
