namespace PyroSnap;

// Diagram: Hotspot
public class Hotspot
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public int Confidence { get; private set; }
    public double Brightness { get; private set; }
    public double Frp { get; private set; }
    public DateTime WaktuAkuisisi { get; private set; }

    public Hotspot(double lat, double lon, int confidence, double brightness, double frp, DateTime waktu)
    {
        Latitude = lat;
        Longitude = lon;
        Confidence = confidence;
        Brightness = brightness;
        Frp = frp;
        WaktuAkuisisi = waktu;
    }

    public string GetDetail()
    {
        return "Lat: " + Latitude
            + ", Lon: " + Longitude
            + ", Confidence: " + Confidence + "%"
            + ", Brightness: " + Brightness + " K"
            + ", FRP: " + Frp + " MW"
            + ", Waktu: " + WaktuAkuisisi.ToString("yyyy-MM-dd HH:mm");
    }
}
