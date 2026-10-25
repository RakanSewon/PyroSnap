using System.Collections.Generic;

namespace PyroSnap.Views
{
    /// <summary>
    /// Penyimpan state ringan antar-tampilan (data hotspot yang sedang
    /// ditampilkan + hotspot yang dipilih untuk halaman Detail/Export).
    /// </summary>
    public static class AppSession
    {
        /// <summary>Hotspot yang sedang tampil setelah filter (dipakai Export).</summary>
        public static List<Hotspot> CurrentHotspots { get; set; } = new List<Hotspot>();

        /// <summary>Hotspot yang dipilih user (dipakai halaman Detail).</summary>
        public static Hotspot? SelectedHotspot { get; set; }
    }
}
