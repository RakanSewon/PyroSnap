using System.Globalization;
using System.Text;
using System.IO;

namespace PyroSnap;

// Diagram: HotspotReport --berisi--> Hotspot (agregasi)
public class HotspotReport
{
    private string isi = "";

    public string Format { get; private set; }
    public DateTime TanggalDibuat { get; private set; }
    public List<Hotspot> DaftarHotspot { get; private set; }

    /// <summary>Teks laporan hasil <see cref="Generate"/> (kosong bila belum dibuat).</summary>
    public string Isi => isi;

    public void MuatData(List<Hotspot> data)
    {
        DaftarHotspot = data;
    }

    public HotspotReport(string format)
    {
        Format = format;
        DaftarHotspot = new List<Hotspot>();
    }

    /// <summary>Susun isi laporan sesuai <see cref="Format"/> (TXT atau CSV).</summary>
    public bool Generate()
    {
        if (DaftarHotspot.Count == 0)
        {
            return false;
        }

        isi = string.Equals(Format, "CSV", StringComparison.OrdinalIgnoreCase)
            ? BuatCsv()
            : BuatTxt();
        TanggalDibuat = DateTime.Now;
        return true;
    }

    /// <summary>
    /// Tulis isi laporan ke file. Kegagalan I/O (file terkunci, tanpa izin)
    /// dilempar sebagai IOException/UnauthorizedAccessException untuk ditangani pemanggil.
    /// </summary>
    public void ExportToFile(string path)
    {
        if (isi.Length == 0)
            throw new InvalidOperationException("Laporan belum dibuat. Panggil Generate() lebih dulu.");

        File.WriteAllText(path, isi);
    }

    private string BuatTxt()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("LAPORAN HOTSPOT (" + Format + ")");
        sb.AppendLine("Jumlah titik panas: " + DaftarHotspot.Count);

        foreach (Hotspot h in DaftarHotspot)
        {
            sb.AppendLine(h.GetDetail());
        }

        return sb.ToString();
    }

    // Angka selalu memakai titik desimal (InvariantCulture) agar kolom CSV
    // tidak pecah pada Windows ber-locale Indonesia.
    private string BuatCsv()
    {
        var inv = CultureInfo.InvariantCulture;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Latitude,Longitude,Confidence,Brightness,FRP,WaktuAkuisisiUTC");

        foreach (Hotspot h in DaftarHotspot)
        {
            sb.AppendLine(string.Join(",",
                h.Latitude.ToString(inv),
                h.Longitude.ToString(inv),
                h.Confidence.ToString(inv),
                h.Brightness.ToString(inv),
                h.Frp.ToString(inv),
                h.WaktuAkuisisi.ToString("yyyy-MM-dd HH:mm", inv)));
        }

        return sb.ToString();
    }
}
