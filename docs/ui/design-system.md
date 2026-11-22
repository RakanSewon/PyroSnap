# Sistem Desain PyroSnap — Inventaris Saat Ini & Bahan Diskusi

> Status: **baseline**. Tema merah-api dipertahankan sementara (keputusan 2026-11-22); **sistem desain baru akan dibahas berikutnya**. Dokumen ini mencatat apa yang ada sekarang supaya diskusi berangkat dari data, bukan dari nol.

## 1. Token yang sudah ada (`PyroSnap/App.xaml`)

| Token | Nilai | Dipakai untuk |
|---|---|---|
| `FireRedColor` / `FireRed` | `#B71C1C` | Header halaman, tombol utama, teks error |
| `FireOrange` | `#E65100` | (didefinisikan, hampir tidak dipakai) |
| `PanelBg` | `#FFFFFF` | Kartu/panel |
| `PageBg` | `#F7F3F0` | Latar halaman |
| `MutedText` | `#6D6D6D` | Teks sekunder, status, petunjuk |
| (di style tombol sekunder) | `#EDE7E3`, hover `#E0D8D2`, teks `#333333` | Tombol sekunder |

## 2. Warna di luar token (harus dipindah, FE-06)

| Nilai | Lokasi | Fungsi |
|---|---|---|
| `#C81E1E` / `rgb(200,30,30)` | `DashboardView.xaml:128`, `DashboardView.xaml.cs:636`, `map.html:54,127` | Tingkat tinggi |
| `#E68C00` / `rgb(230,140,0)` | `DashboardView.xaml:132`, `.cs:637`, `map.html:55,128` | Tingkat sedang/nominal |
| `#E6C800` / `rgb(230,200,0)` | `DashboardView.xaml:136`, `.cs:638`, `map.html:56,129` | Tingkat rendah |
| `rgb(183,28,28)`, `rgb(230,81,0)`, `rgb(191,160,15)` | `DetailView.xaml.cs:28-30` | Tingkat di Detail (**berbeda** dari peta/legenda) |
| `#FFCCCCCC` | `LoginView.xaml:28,32`, `DashboardView.xaml:61` | Garis tepi input |
| `#FFEAF2F8` | `DashboardView.xaml:111` | Latar area peta |
| `#CCFFFFFF` | `DashboardView.xaml:125,144` | Latar legenda/overlay semi-transparan |
| `#FFF7F3F0` | `MainWindow.xaml:10` | Sama dengan `PageBg` (duplikat) |
| `#aad3df`, `#ffffff`, `#000000` | `map.html:10,64,110` | Latar peta, tepi titik, sorotan titik terpilih |

## 3. Komponen yang ada

| Komponen | Definisi | Catatan |
|---|---|---|
| `PrimaryButton` | `App.xaml` — merah, teks putih, sudut 6, hover/press dengan opacity | Tidak ada gaya fokus keyboard dan gaya nonaktif |
| `SecondaryButton` | `App.xaml` — krem, teks gelap, sudut 6 | Tidak ada gaya fokus keyboard dan gaya nonaktif |
| `PageHeader` | `App.xaml` — latar merah, padding 20,14 | – |
| Kartu/panel | Ditulis ulang per halaman: `Border` putih, sudut 10–12, `DropShadowEffect` | Belum jadi style bersama |
| Input | `TextBox`/`PasswordBox` bawaan + border `#CCCCCC` | Belum ada style |
| Tabel | `DataGrid` bawaan | Belum ada style |
| Ikon | Emoji 🔥 🔔 | Bergantung font sistem |

**Tipografi**: font bawaan sistem (Segoe UI di Windows); ukuran yang dipakai 10, 11, 12, 13, 14, 15, 16, 18, 20, 22, 30 px tanpa skala yang jelas.
**Jarak**: margin/padding ditulis langsung (4, 6, 8, 10, 12, 16, 18, 20, 24, 28, 32) tanpa skala.

## 4. Masalah yang perlu dijawab sistem desain baru
1. Warna tingkat (Rendah/Nominal/Tinggi) dengan kontras cukup (≥ 3:1 di peta), konsisten di peta, legenda, daftar, Detail (UX-03, UX-13, A11Y-02).
2. Gaya fokus keyboard dan gaya nonaktif untuk tombol (A11Y-04).
3. Skala tipografi (minimum 12 px) dan skala jarak.
4. Komponen bersama: kartu, input, tabel, bilah status/banner (info, peringatan, error), badge (🔔), label peran.
5. Pembeda visual per peran? (mis. label "Tamu"/"Publik" di header) tanpa membuat tampilan terpecah.
6. Ikon: tetap emoji atau ikon vektor (mis. Segoe Fluent Icons bawaan Windows 11 / Segoe MDL2 Assets di Windows 10).
7. Mode gelap: perlu atau tidak.

## 5. Checklist target
- [x] **DS-01** — Semua warna lewat token `App.xaml` (FE-06), tanpa mengubah tampilan.
- [ ] **DS-02** — Diskusi sistem desain baru: arah visual, palet (termasuk warna tingkat), tipografi, komponen (§4).
- [ ] **DS-03** — Dokumentasikan keputusan DS-02 di file ini (token, komponen, contoh).
- [ ] **DS-04** — Terapkan token & komponen baru ke semua layar dan `map.html`.
- [ ] **DS-05** — Cek kontras dan fokus keyboard setelah DS-04 (A11Y-A1).

## 6. Rujukan
`PyroSnap/App.xaml`, `docs/ui/ux/ux-audit.md` §2 (kontras), `docs/ui/ux/copy-and-accessibility.md`, `docs/frontend/code-structure.md`
