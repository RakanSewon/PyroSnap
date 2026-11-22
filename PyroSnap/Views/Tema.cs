using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace PyroSnap.Views
{
    /// <summary>
    /// DS-06: tema terang/gelap. Kamus warna aktif ada di indeks 0 MergedDictionaries
    /// (App.xaml); mengganti tema = mengganti kamus itu. Pilihan disimpan per komputer di
    /// %AppData%\PyroSnap\settings.json (bukan di database).
    /// </summary>
    public static class Tema
    {
        public const string Terang = "Terang";
        public const string Gelap = "Gelap";

        public static string Aktif { get; private set; } = Terang;

        public static bool IsGelap => Aktif == Gelap;

        private static string BerkasPengaturan => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PyroSnap", "settings.json");

        /// <summary>Baca pilihan tema; bawaan Terang bila belum ada atau berkas rusak.</summary>
        public static string BacaPilihan()
        {
            try
            {
                if (!File.Exists(BerkasPengaturan)) return Terang;
                using var doc = JsonDocument.Parse(File.ReadAllText(BerkasPengaturan));
                return doc.RootElement.TryGetProperty("tema", out var nilai) && nilai.GetString() == Gelap ? Gelap : Terang;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                return Terang;
            }
        }

        private static void SimpanPilihan(string tema)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(BerkasPengaturan)!);
                File.WriteAllText(BerkasPengaturan, JsonSerializer.Serialize(new { tema }));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Gagal menyimpan tidak menghalangi pemakaian; tema tetap berlaku untuk sesi ini.
            }
        }

        /// <summary>Pasang kamus warna tema (tanpa menyimpan pilihan).</summary>
        public static void Terapkan(string tema)
        {
            tema = tema == Gelap ? Gelap : Terang;
            var kamus = Application.Current.Resources.MergedDictionaries;
            var baru = new ResourceDictionary
            {
                Source = new Uri($"/PyroSnap;component/Themes/{tema}.xaml", UriKind.Relative)
            };

            if (kamus.Count > 0) kamus[0] = baru;   // indeks 0 = kamus warna (App.xaml)
            else kamus.Insert(0, baru);
            Aktif = tema;
        }

        /// <summary>Warna tema aktif (SolidColorBrush) dari kunci kamus.</summary>
        public static Color Warna(string kunci) =>
            Application.Current.TryFindResource(kunci) is SolidColorBrush b ? b.Color : Colors.Gray;

        /// <summary>Warna sebagai #RRGGBB untuk map.html.</summary>
        public static string Hex(string kunci)
        {
            Color c = Warna(kunci);
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        /// <summary>
        /// DS-08: warna antarmuka peta (window.PYROSNAP_CONFIG.tema) supaya peta Leaflet
        /// mengikuti tema aplikasi: tile CARTO gelap/terang, legenda, kontrol, dan sorotan.
        /// </summary>
        public static object KonfigurasiPeta() => new
        {
            gelap = IsGelap,
            latar = Hex("MapBg"),
            panel = Hex("PanelBg"),
            teks = Hex("TextPrimary"),
            teksRedup = Hex("MutedText"),
            tepiKartu = Hex("CardBorder"),
            sorot = Hex("InkBg")
        };

        /// <summary>Ganti terang ↔ gelap dan simpan pilihan.</summary>
        public static void Ganti()
        {
            Terapkan(IsGelap ? Terang : Gelap);
            SimpanPilihan(Aktif);
        }
    }
}
