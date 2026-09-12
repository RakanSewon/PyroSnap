namespace PyroSnap;

// Diagram: FilterCriteria --menyaring--> Hotspot
public class FilterCriteria
{
    public DateTime WaktuMulai { get; set; }
    public DateTime WaktuAkhir { get; set; }
    public string Wilayah { get; set; }
    public int ConfidenceMin { get; set; }
    public double FrpMin { get; set; }

    public FilterCriteria()
    {
        Reset();
    }

    public bool IsMatch(Hotspot h)
    {
        // Catatan: atribut wilayah tidak ikut dicek karena Hotspot pada diagram
        // tidak punya atribut wilayah.
        return h.WaktuAkuisisi >= WaktuMulai
            && h.WaktuAkuisisi <= WaktuAkhir
            && h.Confidence >= ConfidenceMin
            && h.Frp >= FrpMin;
    }

    public void Reset()
    {
        WaktuMulai = DateTime.MinValue;
        WaktuAkhir = DateTime.MaxValue;
        Wilayah = "";
        ConfidenceMin = 0;
        FrpMin = 0;
    }
}
