# Sistem Desain PyroSnap

> Status (2026-11-23): **gaya D5 (dashboard kartu seperti gambar referensi), tema terang + gelap, ikon Segoe, logo vektor, dan peta gelap sudah diterapkan di kode (DS-04…09).** Build bersih dan pemeriksaan otomatis lulus (§12), tetapi **belum dilihat di layar Windows**. Semua rasio kontras di bawah **dihitung dengan rumus WCAG** dari nilai di `PyroSnap/Themes/*.xaml`, bukan perkiraan, kecuali yang ditandai.

## 1. Keputusan

| # | Topik | Keputusan | Tanggal |
|---|---|---|---|
| D1 | Arah visual | ~~Rapikan tema api (merah-api + krem)~~ → **diganti D5** | 2026-11-22 |
| D2 | Pembeda tingkat di peta | **Warna gelap + ukuran + garis tepi + label teks** (bukan warna saja) | 2026-11-22 |
| D3 | Tipografi & ikon | **Segoe UI** (bawaan Windows); ikon **Segoe Fluent Icons** (Windows 11) dengan cadangan **Segoe MDL2 Assets** (Windows 10); tanpa berkas font tambahan | 2026-11-22 |
| D4 | Mode gelap | **Ya, ada pilihan terang/gelap** | 2026-11-22 |
| D5 | Gaya visual | **Dashboard kartu sesuai gambar referensi dari tim**: latar abu muda, kartu putih bersudut besar tanpa bayangan berat, **menu samping** (logo, seksi, ikon, item aktif = garis kiri + tebal), **bilah atas** (judul halaman, lonceng, profil berinisial), **kartu KPI** (angka besar + pil kecil), segmen/tab berbentuk pil dengan pilihan aktif hitam, baris terpilih berupa pil gelap. Merah hanya untuk merek (logo) dan tanda bahaya | 2026-11-23 |
| D6 | Teknologi tema | **Tetap .NET 8 LTS** dengan template kontrol WPF ditulis sendiri (bukan gaya Fluent .NET 9) | 2026-11-23 |

Catatan D5: gambar referensi hanya dipakai sebagai acuan **gaya** (tata letak kartu, warna netral, bentuk pil). Isi, ikon, dan teks seluruhnya milik PyroSnap.

## 2. Token warna

Satu daftar kunci, dua nilai. Berkas: `Themes/Terang.xaml` dan `Themes/Gelap.xaml` (37 kunci, hanya `SolidColorBrush`). Kontras di kolom kanan: terang / gelap.

| Token | Terang | Gelap | Dipakai untuk | Kontras (terang / gelap) |
|---|---|---|---|---|
| `PageBg` | `#EEF0EC` | `#111311` | Latar jendela | – |
| `PanelBg` | `#FFFFFF` | `#1B1E1B` | Kartu, menu samping, bilah atas | – |
| `CardBorder` | `#E3E5E0` | `#2A2E29` | Garis tepi kartu 1 px | – |
| `TextPrimary` | `#1A1C19` | `#ECEEEA` | Teks utama | 17,16 / 14,41 di kartu |
| `MutedText` | `#5F645E` | `#A3A8A1` | Keterangan, label | 6,05 / 6,95 di kartu; 5,32 / 6,02 di ubin |
| `InkBg` / `InkFg` | `#1C1D1B` / `#FFFFFF` | `#ECEEEA` / `#111311` | Tombol utama, segmen aktif, avatar, isi slider, garis menu aktif | 16,92 / 15,99 |
| `SecondaryBg` / `HoverBg` / `Fg` | `#EFF1EC` / `#E3E6DF` / `#1A1C19` | `#262A26` / `#30352F` / `#ECEEEA` | Tombol sekunder, ubin, chip, profil | 15,08 / 12,48 |
| `NavHoverBg` | `#F3F4F1` | `#232723` | Hover menu samping | teks 15,54 / 12,98 |
| `InputBg` / `InputBorder` | `#F7F8F5` / `#878B85` | `#222622` / `#7C807A` | Isi dan tepi input | tepi 3,25 / 3,82 (≥ 3) |
| `FocusRing` | `#1F4E79` | `#8AB4F8` | Cincin fokus 2 px | 8,66 / 7,98 |
| `GridLine` | `#ECEDEA` | `#2A2E29` | Garis tabel | – |
| `SelectionBg` / `Fg` | `#1C1D1B` / `#FFFFFF` | `#ECEEEA` / `#111311` | Baris terpilih (pil gelap) | 16,92 / 15,99 |
| `DeltaTurunBg` / `Fg` | `#FDE5E3` / `#A8201A` | `#3F1D1B` / `#FFB4AB` | Pil KPI "keyakinan tinggi", chip "Baru" | 6,06 / 8,82 |
| `DeltaNaikBg` / `Fg` | `#E2F4E5` / `#1D6B31` | `#16351F` / `#8FE3A5` | Pil naik (cadangan) | 5,72 / 8,76 |
| `Brand` / `BrandFg` | `#B71C1C` / putih | `#DC4437` / putih | Lingkaran logo | ikon 6,57 / 4,26; lingkaran vs kartu 6,57 / 3,95 |
| `FireRed` | `#B71C1C` | `#FF8A80` | Teks bahaya, nomor darurat, lencana lonceng | 6,57 / 7,37 |
| `MapBg` / `OverlayBg` | `#E9ECE7` / `#CCFFFFFF` | `#1E2227` / `#CC1B1E1B` | Latar peta, lapisan "memuat" | teks di overlay 16,57 / 14,24 |
| `PesanInfoBg/Fg` | `#EEF3F8` / `#2B4A66` | `#1E2D3B` / `#BBD7F0` | Bilah info | 8,27 / 9,43 |
| `PesanPeringatanBg/Fg` | `#FFF3E0` / `#7A3E00` | `#3A2A12` / `#FFD7A3` | Bilah peringatan | 7,61 / 10,20 |
| `PesanErrorBg/Fg` | `#FDECEA` / `#B71C1C` | `#3F1D1B` / `#FFB4AB` | Bilah error | 5,74 / 8,82 |

Catatan: `Brand` gelap awalnya `#C62828` (hanya 2,99:1 terhadap kartu gelap) lalu dinaikkan ke `#DC4437`.

## 3. Warna dan tanda tingkat keyakinan deteksi (D2)

Kelas LAPAN: Rendah (< 30), Nominal (30–79), Tinggi (≥ 80) — `Views/TingkatHotspot.cs`.

| Tingkat | Terang | Gelap | vs `MapBg` (terang / gelap) | Ukuran (peta / kanvas) | Garis tepi | Label |
|---|---|---|---|---|---|---|
| Tinggi | `#9B1C1C` | `#FF6B5E` | 6,84 / 5,72 | 9 / 11 | **tebal 2,5 px**: `#5A0000` (terang), `#FFFFFF` (gelap) | "Tinggi (≥ 80%)" |
| Nominal | `#B45309` | `#FFA040` | 4,21 / 7,88 | 7 / 9 | 1,5 px warna kartu | "Nominal (30–79%)" |
| Rendah | `#8A6D00` | `#E6C84A` | 4,13 / 9,69 | 5 / 7 | 1,5 px warna kartu | "Rendah (< 30%)" |

Terhadap tile CARTO (warna tile **perkiraan**: terang darat `#FAFAF8`, air `#D4DADC`; gelap darat `#262626`, air `#0E0E0E`): terang 7,80 / 4,81 / 4,71 di darat dan 5,77 / 3,55 / 3,48 di air; gelap 5,42 / 7,46 / 9,17 di darat. Nominal dan Rendah hampir sama terang, jadi **ukuran + tepi + label tetap wajib**. Warna tingkat dipakai pada penanda, bukan sebagai warna teks.

## 4. Tipografi, jarak, sudut

| Peran | Ukuran | Berat | Style |
|---|---|---|---|
| Keterangan | 12 px | Normal, `MutedText` | `Keterangan` |
| Isi | 13 px | Normal | bawaan halaman |
| Judul seksi menu | 12 px | SemiBold, `MutedText` | `LabelSeksi` |
| Judul kartu | 16 px (14 di kolom filter) | SemiBold | `JudulKartu` |
| Judul halaman (bilah atas) | 20 px | SemiBold | di `MainWindow.xaml` |
| Judul Masuk/Daftar | 22 px | SemiBold | – |
| Angka KPI | 28 px (24 di Detail) | SemiBold | `AngkaKpi` |

Jarak (px): **4, 8, 12, 16, 20, 24**; antarkartu 16 (12 di baris KPI). Sudut: `RadiusKontrol` 12 (tombol, input, bilah pesan, ubin), `RadiusKartu` 20 (kartu), `RadiusPil` 999 (segmen, chip, profil).

## 5. Komponen (`Themes/Gaya.xaml`)

| Komponen | Spesifikasi |
|---|---|
| `Kartu` / `KartuBesar` | `PanelBg`, tepi `CardBorder` 1 px, sudut 20, padding 20 / 28, tanpa bayangan |
| Menu samping (`NavItem`, `NavAksi`) | RadioButton/Button: ikon + teks; hover `NavHoverBg`; **aktif: garis kiri `InkBg` + teks SemiBold**; `NavAksi` untuk Tema dan Keluar |
| Bilah atas | Judul + subjudul per halaman, `TombolIkon` lonceng dengan **lencana angka** (`FireRed`), pil profil (avatar inisial di `InkBg`, nama, peran) |
| `PrimaryButton` | `InkBg`/`InkFg`, sudut 12, padding 18,9; hover/ditekan lewat opacity; nonaktif 0,45; cincin fokus |
| `SecondaryButton` | `SecondaryBg`, hover `SecondaryHoverBg` |
| `TombolIkon` | Lingkaran 40 px, glyph Segoe |
| `WadahSegmen` + `Segmen` | Pilihan tunggal berbentuk pil; aktif `InkBg`/`InkFg` (format ekspor, status verifikasi) |
| Tab (`TabItem`) | Pil seperti segmen |
| Kartu KPI | Keterangan → angka besar → pil kecil (mis. persen keyakinan tinggi) → keterangan |
| `BilahPesan` + `IkonPesan` | Bilah sudut 12; ikon Info/Peringatan/Error diisi `PesanUi` sesuai jenis |
| `Chip` | Pil kecil `SecondaryBg` (status peringatan; "Baru" memakai `DeltaTurun*`) |
| Input | Template sendiri: TextBox, PasswordBox, ComboBox (termasuk popup dan item), DatePicker (kotak teks) |
| `DataGrid` | Header 12 px SemiBold `MutedText`, garis horizontal `GridLine`, baris terpilih pil gelap |
| `Slider` | Lintasan 4 px, isi `InkBg`, kenop bulat 18 px |
| `ScrollBar` | Tipis 10 px, tanpa panah, kenop membulat |
| `ToolTip`, `Separator` | Warna tema |

## 6. Ikon dan logo (D3, DS-09)

- Font: `IkonFont` = `Segoe Fluent Icons, Segoe MDL2 Assets`. Glyph dikumpulkan di `Views/Ikon.cs`; di XAML ditulis sebagai entitas yang sama.

| Arti | Glyph | Dipakai di |
|---|---|---|
| Beranda/Dashboard | `E80F` | menu |
| Lonceng | `E7ED` | menu, bilah atas, kartu info peringatan |
| Unduh/Ekspor | `E896` | menu, tombol Ekspor |
| Bantuan | `E897` | menu Panduan, tautan di Masuk |
| Info / Peringatan / Error | `E946` / `E7BA` / `EA39` | bilah pesan |
| Centang | `E73E` | Simpan verifikasi, Tandai semua |
| Cari | `E721` | Cari lokasi |
| Segarkan | `E72C` | Perbarui dari NASA FIRMS |
| Kembali | `E72B` | tombol Kembali |
| Salin | `E8C8` | Salin koordinat |
| Kalender | `E787` | Tampilkan rentang |
| Lokasi | `E81D` | Buka arah, Buka titik panas |
| Matahari / Bulan | `E706` / `E708` | tombol tema |
| Keluar | `E7E8` | menu |
| Panah | `E70D` (bawah), `E76C` (kanan) | ComboBox, Lihat detail |

- **Logo**: emoji 🔥 diganti `Geometry LogoNyala` (nyala dua lapis) di lingkaran `Brand`; dipakai di menu samping, Masuk, Daftar, dan Panduan tanpa sesi.
- Tombol ikon selalu punya `AutomationProperties.Name` atau tooltip.
- **Perlu dicek di Windows 10 dan 11** (Character Map): semua glyph di atas tampil dan artinya sesuai.

## 7. Tata letak halaman (D5)

- **Shell** (`MainWindow`): kolom menu samping 240 px + area kanan (bilah atas + Frame). Menu sesuai peran: Peringatan hanya Admin/Operator, Ekspor hanya yang boleh ekspor. Shell disembunyikan di Masuk, Daftar, dan Panduan tanpa sesi.
- **Dashboard**: baris 4 KPI (titik tampil, keyakinan tinggi + %, area pantau terdampak, intensitas tertinggi) → bilah status + tombol Ekspor / Perbarui → peta (atas) dan daftar dengan tab pil (bawah) di kiri, kolom filter 290 px di kanan. Semua KPI dihitung dari titik yang sedang tampil (`DashboardView.Kpi.cs`).
- **Detail**: kiri data titik (3 ubin angka + label/nilai + navigasi); kanan verifikasi lapangan dengan segmen status.
- **Peringatan**: 2 KPI (belum dibaca, total) + kartu info → daftar dengan chip status.
- **Ekspor**: kartu KPI jumlah titik + segmen format → pratinjau.
- **Panduan**: dua kolom; nomor darurat sebagai ubin.
- **Masuk/Daftar**: kartu tengah dengan logo; tombol tema di pojok kanan atas Masuk.
- Peta WebView2 tidak bisa ikut sudut membulat (masalah *airspace*), jadi diberi jarak 6 px di dalam kartu.

## 8. Mekanisme tema (DS-06)

1. `App.xaml` menggabungkan **indeks 0 = kamus warna** (`Themes/Terang.xaml` atau `Gelap.xaml`) dan indeks 1 = `Themes/Gaya.xaml` (bentuk, style, template).
2. Semua warna dirujuk lewat `DynamicResource`; `StaticResource` hanya untuk yang bukan warna (sudut, font, geometri, style).
3. `Views/Tema.cs`: `BacaPilihan()` dari `%AppData%\PyroSnap\settings.json` (`{"tema":"Gelap"}`; bawaan Terang; berkas rusak = Terang), `Terapkan()` mengganti kamus indeks 0, `Ganti()` menukar dan menyimpan (gagal simpan tidak menghentikan aplikasi).
4. `App.OnStartup` memasang tema tersimpan sebelum jendela dibuat.
5. Tombol tema di menu samping (dan di halaman Masuk). Setelah ganti, **halaman aktif dimuat ulang** (instance baru) supaya warna yang disetel dari kode dan peta ikut berubah; data/filter tetap karena tersimpan di `AppSession`. Detail memakai `Salinan()` agar tujuan tombol Kembali tidak hilang.
6. Peta (DS-08): `window.PYROSNAP_CONFIG.tema` (`Tema.KonfigurasiPeta()`) berisi `gelap`, warna latar/panel/teks/tepi, dan warna sorot. `map.html` memilih tile CARTO `dark_all`/`light_all`, mewarnai legenda, kontrol zoom/layer, dan atribusi lewat variabel CSS, dan menyorot titik terpilih dengan warna tinta tema. `WebView2.DefaultBackgroundColor` = `MapBg` (tanpa kilatan putih). Peta kanvas memakai warna teks tema untuk garis lintang/bujur.

## 9. Checklist target

- [x] **DS-01** — Semua warna lewat token (FE-06).
- [x] **DS-02** — Diskusi dan keputusan arah (§1).
- [x] **DS-03** — Keputusan didokumentasikan.
- [x] **DS-04** — Tema terang, skala tipografi/jarak, style bersama, cincin fokus (diperbarui ke D5 2026-11-23).
- [x] **DS-05** — Warna dan tanda tingkat di peta, kanvas, legenda, daftar, Detail.
- [x] **DS-06** — Struktur tema: dua kamus berkunci sama, `DynamicResource`, simpan pilihan, tombol pengganti, muat ulang halaman.
- [x] **DS-07** — Tema gelap dengan template kontrol sendiri di .NET 8 (D6): tombol, menu, input, ComboBox, DatePicker (kotak teks), DataGrid, tab, slider, scrollbar, tooltip.
- [x] **DS-08** — Peta gelap (`dark_all`, legenda, kontrol, sorotan) + konfigurasi `tema`.
- [x] **DS-09** — Ikon Segoe + logo vektor + ikon di bilah pesan. *Glyph masih perlu dicek di Windows.*
- [ ] **DS-10** — Uji kedua tema. Otomatis sudah lulus (§12); **sisa: cek di layar Windows** semua halaman × semua peran × kedua tema, catat di `docs/debug/testing-schema.md`.

## 10. Hasil penerapan DS-04/DS-05 awal (2026-11-23, sebelum D5)

Tema api (merah + krem) diterapkan lebih dulu: token terpusat, style `Kartu`, template TextBox/PasswordBox, gaya DataGrid/Tab, penanda tingkat dengan ukuran + tepi. Pada hari yang sama arah visual diganti D5; tokennya kini mengikuti §2 dan style `PageHeader`/`HeaderFg` dihapus.

## 11. Batasan yang diketahui

- **Belum dilihat di Windows**: tampilan, glyph, cincin fokus, template ComboBox/DatePicker/Slider/ScrollBar perlu dicek langsung.
- **Kalender DatePicker** (popup) masih bawaan WPF, jadi tampil terang di tema gelap. Kotak teksnya sudah bergaya.
- Sudut kotak antar-scrollbar (`ScrollViewer`) memakai warna sistem.
- Ukuran jendela minimum 1024 × 640; di ukuran itu peta dan daftar cukup sempit.
- Warna tile CARTO di §3 adalah perkiraan; kontras penanda di atas tile perlu dicek di layar.

## 12. Hasil verifikasi otomatis (Linux, 2026-11-23)

- `dotnet build` (.NET 8, `EnableWindowsTargeting`): **0 error**, 2 peringatan lama (`FilterCriteria.Wilayah`, `MapViewer.MapCenter`).
- Pemeriksa sumber daya (skrip Python): kunci Terang = Gelap; semua XAML valid; semua `StaticResource` di `Gaya.xaml` sudah didefinisikan lebih dulu; tidak ada warna tema lewat `StaticResource`; semua kunci yang dirujuk XAML dan C# ada; tidak ada sisa `PageHeader`/`HeaderFg`. Pemeriksa diuji balik dengan kunci palsu dan rujukan statis (keduanya tertangkap).
- Kontras: **68 pasangan** (34 per tema) memenuhi ambang (teks 4,5:1; grafis/tepi 3:1).
- Uji `map.html` dengan stub Leaflet (Node): mode tanpa konfigurasi, terang, dan gelap — masing-masing **9/9** (tile, variabel CSS, legenda, ukuran/tepi penanda, sorotan).

## 13. Keputusan terbuka

1. Opsi "ikuti tema Windows"? Usulan: tidak untuk MVP.
2. Bentuk logo vektor — perlu persetujuan tim.
3. Uji kontras di layar dengan pengguna buta warna / kecerahan rendah (FLOW-A3)?

## 14. Rujukan

`PyroSnap/Themes/Terang.xaml`, `Gelap.xaml`, `Gaya.xaml`, `PyroSnap/Views/Tema.cs`, `Ikon.cs`, `PesanUi.cs`, `TingkatHotspot.cs`, `DashboardView.Kpi.cs`, `PyroSnap/MainWindow.xaml`, `PyroSnap/Assets/map.html`, `docs/ui/ux/copy-and-accessibility.md`, `docs/MVP.md`
