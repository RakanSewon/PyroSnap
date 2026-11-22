using System;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// Pembungkus tampilan satu Hotspot untuk DataGrid. Properti teks untuk ditampilkan,
    /// properti angka/waktu untuk pengurutan (UX-05: urut numerik, bukan teks).
    /// </summary>
    public class HotspotRow
    {
        public Hotspot Source { get; }

        public HotspotRow(Hotspot h)
        {
            Source = h;
            Wilayah = WilayahPantau.Temukan(h)?.NamaLokasi ?? "Di luar area pantau";
        }

        // Nilai untuk pengurutan (SortMemberPath)
        public int Confidence => Source.Confidence;
        public double Frp => Source.Frp;
        public double Brightness => Source.Brightness;
        public DateTime Waktu => Source.WaktuAkuisisi;

        // Teks tampilan
        public string WaktuText => Source.WaktuAkuisisi.ToLocalTime().ToString("dd MMM HH:mm");
        public string Koordinat => $"{Source.Latitude:0.0000}, {Source.Longitude:0.0000}";
        public string ConfidenceText => Source.Confidence + " %";
        public string FrpText => $"{Source.Frp:0.##} MW";
        public string BrightnessText => $"{Source.Brightness:0.#} K";
        public string Tingkat => TingkatHotspot.Nama(TingkatHotspot.Dari(Source.Confidence));

        // Penanda tingkat di daftar (DS-05): warna + ukuran + tepi, sama dengan peta
        public System.Windows.Media.Brush WarnaTingkat => TingkatHotspot.Warna(TingkatHotspot.Dari(Source.Confidence));
        public System.Windows.Media.Brush TepiTingkat => TingkatHotspot.Tepi(TingkatHotspot.Dari(Source.Confidence));
        public double TebalTepiTingkat => TingkatHotspot.TebalTepi(TingkatHotspot.Dari(Source.Confidence));
        public double UkuranTitik => TingkatHotspot.UkuranTitik(TingkatHotspot.Dari(Source.Confidence));
        public string Wilayah { get; }
    }
}
