using System;
using System.Collections.Generic;

namespace PyroSnap.Views
{
    /// <summary>
    /// Penyimpan state ringan antar-tampilan: sesi (user atau Tamu), hak akses per peran,
    /// data yang dimuat, status, dan filter. Dashboard dibuat ulang setiap navigasi, jadi
    /// kembali dari Detail/Ekspor tidak memuat ulang data atau menghapus filter.
    /// </summary>
    public static class AppSession
    {
        // ===== Sesi =====

        /// <summary>User yang sedang login; null bila belum login atau sesi Tamu.</summary>
        public static User? CurrentUser { get; set; }

        /// <summary>Warga tanpa akun (K10): hanya baca, tanpa ekspor, tanpa baris di tabel users.</summary>
        public static bool IsTamu { get; private set; }

        public static void MasukSebagaiTamu()
        {
            Bersihkan();
            IsTamu = true;
        }

        public static bool AdaSesi => IsTamu || CurrentUser is { SudahLogin: true };

        /// <summary>
        /// Peran efektif: Admin, Operator, Publik, atau Tamu. Akun "Analis" (sebelum migrasi 008)
        /// diperlakukan sebagai Admin (K9: Admin dan Analis digabung).
        /// </summary>
        public static string Peran =>
            IsTamu ? "Tamu"
            : CurrentUser?.Role == "Analis" ? "Admin"
            : CurrentUser?.Role ?? "";

        public static int? UserIdAngka =>
            int.TryParse(CurrentUser?.UserId, out int id) ? id : null;

        // ===== Hak akses (RUI-01 / ROLE-02), satu sumber untuk semua layar =====

        private static bool IsAdmin => Peran == "Admin";
        private static bool IsOperator => Peran == "Operator";

        /// <summary>Publik atau Tamu: mendapat banner peringatan warga dan wilayah pilihan.</summary>
        public static bool IsWarga => Peran is "Publik" or "Tamu";

        public static bool BolehSinkron => IsAdmin;
        public static bool BolehEkspor => IsAdmin || IsOperator || Peran == "Publik";
        public static bool TerimaAlertOperasional => IsAdmin || IsOperator;
        public static bool BolehVerifikasi => IsAdmin || IsOperator;
        public static bool BolehNavigasi => IsAdmin || IsOperator;
        public static bool BolehHistoris => IsAdmin || IsOperator;

        /// <summary>Wilayah pilihan bisa disimpan permanen hanya untuk akun Publik.</summary>
        public static bool WilayahTersimpan => Peran == "Publik";

        // ===== Data & tampilan =====

        /// <summary>Hotspot yang sedang tampil setelah filter (dipakai Ekspor).</summary>
        public static List<Hotspot> CurrentHotspots { get; set; } = new List<Hotspot>();

        /// <summary>Hotspot yang dipilih user (dipakai halaman Detail).</summary>
        public static Hotspot? SelectedHotspot { get; set; }

        /// <summary>Data terbaru (dari NASA FIRMS atau database).</summary>
        public static List<Hotspot> SemuaHotspot { get; set; } = new List<Hotspot>();

        /// <summary>true bila data sudah pernah berhasil dimuat pada sesi ini.</summary>
        public static bool DataDimuat { get; set; }

        /// <summary>Nama sumber data (kunci hotspot_data_source), mis. "NASA FIRMS (VIIRS_NOAA20_NRT)".</summary>
        public static string? NamaSumber { get; set; }

        /// <summary>URL sumber dengan penanda {KEY} (aman disimpan di database).</summary>
        public static string? EndpointTanpaKey { get; set; }

        /// <summary>Status pemuatan terakhir (UX-17: ringkas + rincian).</summary>
        public static string StatusUtama { get; set; } = "";
        public static string StatusDetail { get; set; } = "";
        public static JenisPesan StatusJenis { get; set; } = JenisPesan.Info;

        /// <summary>Data historis yang sedang ditampilkan (OPS-03); null = data terbaru.</summary>
        public static (DateTime Dari, DateTime Sampai)? RentangHistoris { get; set; }
        public static List<Hotspot> DataHistoris { get; set; } = new List<Hotspot>();

        /// <summary>Kriteria filter aktif (confidence, FRP).</summary>
        public static FilterCriteria Filter { get; set; } = new FilterCriteria();

        /// <summary>Lokasi hasil pencarian yang sedang difokuskan; null = seluruh wilayah.</summary>
        public static Location? AreaFokus { get; set; }

        public static string TeksCari { get; set; } = "";

        /// <summary>Wilayah pilihan warga (PUB-01); null bila belum memilih.</summary>
        public static string? WilayahPilihan { get; set; }

        /// <summary>true bila wilayah pilihan akun Publik sudah dibaca dari database.</summary>
        public static bool WilayahPilihanDimuat { get; set; }

        /// <summary>Kosongkan semua state dan logout user aktif.</summary>
        public static void Bersihkan()
        {
            CurrentUser?.Logout();
            CurrentUser = null;
            IsTamu = false;
            CurrentHotspots = new List<Hotspot>();
            SelectedHotspot = null;
            SemuaHotspot = new List<Hotspot>();
            DataDimuat = false;
            NamaSumber = null;
            EndpointTanpaKey = null;
            StatusUtama = "";
            StatusDetail = "";
            StatusJenis = JenisPesan.Info;
            RentangHistoris = null;
            DataHistoris = new List<Hotspot>();
            Filter = new FilterCriteria();
            AreaFokus = null;
            TeksCari = "";
            WilayahPilihan = null;
            WilayahPilihanDimuat = false;
        }
    }
}
