namespace PyroSnap;

// Diagram: Location
public class Location
{
    public string NamaLokasi { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public Location(string nama, double lat, double lon)
    {
        NamaLokasi = nama;
        Latitude = lat;
        Longitude = lon;
    }

    public string GetCoordinates()
    {
        return Latitude + ", " + Longitude;
    }
}
