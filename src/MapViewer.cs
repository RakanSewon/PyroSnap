namespace PyroSnap;

// Diagram: Dashboard / MapViewer
public class MapViewer
{
    private List<Hotspot> pin = new List<Hotspot>();
    private FilterCriteria filter = new FilterCriteria();

    public int CurrentZoomLevel { get; set; }
    public Location MapCenter { get; set; }

    public void RenderMap()
    {
        Console.WriteLine("=== PETA TITIK PANAS ===");

        if (MapCenter != null)
        {
            Console.WriteLine("Pusat: " + MapCenter.NamaLokasi + " (" + MapCenter.GetCoordinates() + ")");
        }

        Console.WriteLine("Zoom: " + CurrentZoomLevel);
        Console.WriteLine("Jumlah pin: " + pin.Count);

        foreach (Hotspot h in pin)
        {
            Console.WriteLine("- " + h.GetDetail());
        }
    }

    public void PinHotspot(Hotspot h)
    {
        if (filter.IsMatch(h))
        {
            pin.Add(h);
        }
    }

    public void ApplyFilter(FilterCriteria f)
    {
        filter = f;

        // Pin yang sudah terpasang disaring ulang dengan filter baru.
        List<Hotspot> hasil = new List<Hotspot>();
        foreach (Hotspot h in pin)
        {
            if (filter.IsMatch(h))
            {
                hasil.Add(h);
            }
        }

        pin = hasil;
    }
}
