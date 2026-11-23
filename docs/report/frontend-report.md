# Laporan Frontend PyroSnap

> Kondisi per 2026-11-23. Rincian: `docs/progress/frontend-progress.md`, `docs/frontend/`, `docs/ui/design-system.md`.

## 1. Teknologi

| Bagian | Pilihan |
|---|---|
| UI | WPF, XAML dengan code-behind |
| Peta | Leaflet 1.9.4 di dalam WebView2 |
| Peta dasar | Esri Light/Dark Gray Canvas dan citra satelit, tanpa key |
| Tema | Terang dan gelap, pilihan tersimpan di `%AppData%\PyroSnap\settings.json` |
| Ikon dan huruf | Segoe Fluent Icons/MDL2 dan Segoe UI |

## 2. Halaman

| Halaman | Isi |
|---|---|
| Masuk | Login, masuk sebagai Tamu, status database, ganti tema |
| Daftar | Pendaftaran akun warga |
| Dashboard | KPI, peta, daftar titik, ringkasan wilayah, filter, data historis |
| Detail | Data titik, kondisi sekitar dari web API, verifikasi lapangan |
| Peringatan | KPI, daftar peringatan, tandai dibaca |
| Ekspor Laporan | Pratinjau dan simpan CSV/TXT |
| Panduan | Kontak darurat dan langkah saat ada asap, bisa dibuka tanpa login |

## 3. Hak akses di tampilan

| Fitur | Admin | Operator | Publik | Tamu |
|---|:---:|:---:|:---:|:---:|
| Peta, daftar, Detail, Panduan | ✓ | ✓ | ✓ | ✓ |
| Kondisi sekitar | ✓ | ✓ | ✓ | ✓ |
| Wilayah saya | – | – | ✓ (tersimpan) | ✓ (satu sesi) |
| Ekspor laporan | ✓ | ✓ | ✓ | – |
| Peringatan operasional | ✓ | ✓ | – | – |
| Verifikasi lapangan, navigasi ke titik | ✓ | ✓ | – | – |
| Data historis | ✓ | ✓ | – | – |
| Sinkron NASA FIRMS | ✓ | – | – | – |

## 4. Sistem desain

- Tata letak dashboard kartu: menu samping, bilah atas dengan lonceng dan profil, serta kartu KPI.
- Warna memakai 37 kunci yang sama di tema terang dan gelap.
- Tingkat titik dibedakan dengan warna, ukuran, tepi, dan label, sehingga tidak bergantung pada warna saja.
- Sudut memakai persegi membulat, dan peta mendapat porsi 2:1 terhadap daftar.
- Titik Tinggi selalu digambar paling atas, dan legenda diletakkan agar tidak menutupi titik.

## 5. Hasil uji

| Uji | Hasil |
|---|---|
| Build | 0 error, 2 peringatan lama |
| Kelengkapan kunci tema dan 68 pasangan kontras | Lulus |
| `map.html` dalam 4 mode | Lulus |
| Render 691 titik FIRMS asli di browser, tema terang dan gelap | Lulus |
| Tes awal paket `.exe` di Windows | Lulus 5/5 |

Perbaikan tampilan dari pengujian tercatat di `docs/debug/bug-fix.md`, terutama BUG-21 sampai BUG-36.

## 6. Batasan

- Popup kalender DatePicker tetap terang pada tema gelap.
- Peta dasar dan Leaflet membutuhkan internet.
- Tanpa WebView2 Runtime, aplikasi memakai peta kanvas sederhana.
- Aplikasi hanya berjalan di Windows.
