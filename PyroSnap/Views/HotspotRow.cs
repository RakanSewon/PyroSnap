using System;

namespace PyroSnap.Views
{
    /// <summary>
    /// Pembungkus tampilan (presentation) untuk satu Hotspot agar mudah
    /// ditampilkan di DataGrid. Tidak mengubah model Hotspot milik tim.
    /// </summary>
    public class HotspotRow
    {
        public Hotspot Source { get; }

        public HotspotRow(Hotspot h)
        {
            Source = h;
        }

        public string Koordinat => $"{Source.Latitude:0.0000}, {Source.Longitude:0.0000}";
        public string ConfidenceText => Source.Confidence + " %";
        public string FrpText => Source.Frp + " MW";
        public string BrightnessText => Source.Brightness + " K";

        public string Tingkat
        {
            get
            {
                if (Source.Confidence >= 80) return "Tinggi";
                if (Source.Confidence >= 50) return "Sedang";
                return "Rendah";
            }
        }
    }
}
