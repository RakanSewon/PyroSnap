# Progres Frontend & UI/UX PyroSnap

> Papan pelacakan. Dibuat 2026-11-22. Backend/database ada di `backend-progress.md` (Fase 7 di sana berisi pekerjaan data untuk fitur yang sama).
> Keputusan dasar (2026-11-22): tema visual dipertahankan dulu, **sistem desain baru dibahas berikutnya**; arsitektur **code-behind dirapikan** (bukan MVVM); akun warga lewat **daftar mandiri (boleh ekspor)** dan **masuk sebagai tamu (tanpa ekspor)**.

## 1. Ringkasan fase

| Fase | Nama | Status | Dokumen utama | Bergantung pada |
|---|---|---|---|---|
| FE0 | Rapikan struktur kode tampilan | 🟨 kode selesai, build 0 error; uji Windows menunggu | `frontend/code-structure.md` | – |
| FE1 | Perbaikan UX layar yang ada | 🟨 kode selesai; uji Windows & persetujuan teks menunggu | `ui/ux/ux-audit.md`, `ui/ux/copy-and-accessibility.md` | FE0 (FE-01, FE-05) |
| FE2 | Peran, Tamu & Daftar | 🟨 kode selesai, matriks peran teruji (7/7); migrasi 008 & uji Windows menunggu | `frontend/role-based-ui.md`, `frontend/screens-and-navigation.md` | backend F7: ROLE-01…05 |
| FE3 | Layar & fitur baru (warga dan lapangan) | 🟨 kode selesai, repository teruji (37/37); migrasi 009 & uji Windows menunggu | `frontend/screens-and-navigation.md`, `ui/ux/user-flows.md` | backend F7: OPS-01…03, PUB-01…03; keputusan K2 |
| FE4 | Sistem desain baru | 🟨 DS-02…09 selesai di kode (gaya D5 + tema terang/gelap + ikon + peta gelap, 2026-11-23); DS-10 sisa cek di layar Windows | `ui/design-system.md` | DS-02 |
| FE5 | Uji UI di Windows | 🟨 uji logika di Linux lulus; uji Windows menunggu | `debug/testing-schema.md`, `ui/ux/user-flows.md` | FE0–FE3 |

Legenda: ⬜ belum mulai · 🟨 berjalan · ✅ selesai & terverifikasi · ⏸ menunggu keputusan.

## 2. Checklist per fase

### FE0 — Rapikan struktur kode tampilan
- [x] FE-01 — `TingkatHotspot` satu sumber (kelas LAPAN, warna, ukuran) + dikirim ke `map.html`
- [x] FE-02 — `DashboardView` dipecah jadi partial class (Data, Filter, Peta)
- [x] FE-03 — `Navigasi` terpusat + penjaga sesi/peran (SCR-A1)
- [x] FE-04 — Hak akses UI dari `AppSession` saja (RUI-01)
- [x] FE-05 — `PesanUi`: info/peringatan/error inline; MessageBox hanya konfirmasi & error kritis
- [x] FE-06 / DS-01 — Semua warna hardcoded pindah ke token `App.xaml`
- [x] FE-07 — Ukuran minimum jendela + panel kiri bisa di-scroll (UX-01)
- [x] FE-08, FE-09 — Pola async diperiksa; build bersih; perilaku tidak berubah

### FE1 — Perbaikan UX layar yang ada
- [x] UX-03 — Kelas tingkat sesuai LAPAN (Rendah < 30 / Nominal 30–79 / Tinggi ≥ 80) di semua tempat
- [x] UX-04 — "Tingkat Keparahan" → "Keyakinan deteksi" (BUG-18)
- [x] UX-08 — Status memuat/gagal jelas di luar area peta, teks ≥ 12 px
- [x] UX-18 — Penjelasan istilah untuk warga
- [x] UX-02 — MessageBox rutin → pesan inline
- [x] UX-05 — Urut numerik + kolom waktu (BUG-09)
- [x] UX-09 — Enter untuk Login
- [x] UX-11 — Dari Alert bisa membuka hotspotnya
- [x] UX-12 — Istilah seragam sesuai glosarium (diterapkan; persetujuan tim CPY-01 menunggu)
- [x] UX-13, UX-15, UX-16 — Bukan hanya warna, teks ≥ 12 px, keyboard (A11Y-01…05)
- [x] UX-10, UX-17 — Pratinjau ekspor dibatasi, status dipecah

### FE2 — Peran, Tamu & Daftar
- [x] RUI-01 — Properti hak akses lengkap di `AppSession`
- [x] RUI-02 / SCR-A3 — Sesi Tamu (label "Tamu", tombol "Keluar", tanpa ekspor/alert)
- [x] SCR-A2 — Layar Daftar (Publik), validasi di tempat, langsung masuk setelah berhasil
- [x] RUI-03, RUI-04 — Dashboard & Detail menampilkan elemen sesuai peran
- [x] RUI-05 / SCR-A9 — Penjaga di halaman terbatas (Ekspor untuk Tamu, Alert untuk Publik/Tamu)

### FE3 — Layar & fitur baru
- [x] SCR-A4 / PUB-01 — Wilayah pilihan + banner peringatan warga (teks disetujui: CPY-02)
- [x] SCR-A5 / PUB-02 — Ringkasan risiko per wilayah (butuh K2)
- [x] SCR-A6 / PUB-03 — Halaman Panduan & kontak darurat (isi resmi & dicek: CPY-03)
- [x] SCR-A7 / OPS-01, OPS-02 — Verifikasi lapangan + navigasi di Detail
- [x] SCR-A8 / OPS-03 — Rentang tanggal data historis
- [ ] SCR-14, SCR-15 — Kelola akun & riwayat laporan (⏳ belum diputuskan masuk F7)

### FE4 — Sistem desain baru
- [x] DS-02, DS-03 — Keputusan: rapikan tema api; tingkat = warna gelap + ukuran + tepi + label; Segoe UI + ikon Segoe Fluent/MDL2; **mode gelap ya** (dibangun bertahap, gelap terakhir) — lihat `ui/design-system.md`
- [x] DS-04 — Tema terang: token, skala tipografi/jarak, style bersama (kartu, input, tabel, tab), cincin fokus, `InputBorder` `#8A8A8A`
- [x] DS-05 — Warna & tanda tingkat baru di peta/kanvas/legenda/daftar/Detail; cek kontras & fokus di Windows
- [x] D5 — Gaya dashboard kartu sesuai gambar referensi: shell menu samping + bilah atas (lonceng berlencana, profil), KPI Dashboard/Peringatan/Ekspor/Detail, segmen pil; semua halaman ditulis ulang
- [x] DS-06 — Struktur tema (`Themes/Terang.xaml` + `Gelap.xaml` + `Gaya.xaml`, `DynamicResource`, `Views/Tema.cs`, simpan pilihan, tombol di menu samping & Masuk, muat ulang halaman)
- [x] DS-07 — Tema gelap + template kontrol sendiri di .NET 8 (D6): input, ComboBox, DatePicker (kotak teks), DataGrid, tab, slider, scrollbar
- [x] DS-08 — Peta gelap (tile `dark_all`, legenda, kontrol, sorotan) + `PYROSNAP_CONFIG.tema`
- [x] DS-09 — Ikon Segoe (`Views/Ikon.cs`) + logo vektor + ikon di bilah pesan (glyph perlu dicek di Windows)
- [ ] DS-10 — Uji kedua tema: otomatis lulus; **sisa cek di layar Windows** (semua halaman & peran)

### FE5 — Uji UI di Windows
- [x] Uji logika di Linux: parser 30/30 (pemetaan l=20), F7 backend 37/37, matriks peran 7/7, script peta 10/10, build 0 error
- [ ] RUI-06 — Uji tampilan per peran (Admin, Operator, Publik, Tamu)
- [ ] FLOW-A1, FLOW-A2 — Semua alur & kondisi gagal
- [ ] FLOW-A3 — Uji singkat dengan 1–2 orang di luar tim
- [ ] A11Y-A1 — Cek aksesibilitas (termasuk Narrator)

## 3. Keputusan

| # | Keputusan | Status |
|---|---|---|
| FK1 | Arah visual | ✅ rapikan tema api (D1); tipografi Segoe UI + ikon Segoe (D3); tingkat = warna gelap + ukuran + tepi + label (D2); mode gelap ya, bertahap (D4) |
| FK2 | Arsitektur frontend | ✅ code-behind dirapikan (partial class + helper kecil), bukan MVVM |
| FK3 (= K10) | Cara warga masuk | ✅ daftar mandiri (Publik, boleh ekspor) + masuk sebagai tamu (tanpa ekspor) |
| FK4 | Ringkasan & wilayah pilihan: panel di Dashboard atau halaman sendiri | ✅ diterapkan: tab "Ringkasan per Wilayah" + panel "Wilayah Saya" |
| FK5 | Panduan bisa dibuka tanpa login | ✅ diterapkan (tombol di halaman Masuk) |
| FK6 | Kelola akun & riwayat laporan masuk F7 | ⬜ |
| FK7 | Filter langsung berlaku atau tetap tombol "Terapkan" | ✅ tetap tombol |
| FK8 | Glosarium & teks peringatan warga | 🟨 sudah diterapkan; menunggu persetujuan tim (CPY-01, CPY-02) |

## 4. Log perubahan

| Tanggal | Perubahan | Oleh | Commit |
|---|---|---|---|
| 2026-11-22 | Dokumen rencana frontend & UI/UX dibuat (tanpa perubahan kode) | – | – |
| 2026-11-23 | Gaya D5 + DS-06…09: shell menu samping/bilah atas, semua halaman ditulis ulang ke kartu, KPI, tema terang/gelap, ikon Segoe, logo vektor, peta gelap, template Slider/ScrollBar. Build 0 error; pemeriksa kunci & 68 pasangan kontras lulus; uji peta 3 mode 9/9; belum dilihat di Windows | – | belum di-commit |
| 2026-11-23 | DS-04/DS-05: token & style bersama di `App.xaml` (kartu, input dengan cincin fokus, tabel, tab, fokus tombol), skala huruf/jarak di semua halaman, palet tingkat gelap + ukuran + tepi di peta/kanvas/legenda/daftar. Build 0 error, kunci sumber daya 37/37, script peta 7/7; belum dilihat di Windows | – | belum di-commit |
| 2026-11-22 | Keputusan sistem desain D1–D4 dicatat di `ui/design-system.md` (token terang/gelap dengan rasio kontras terhitung, warna & tanda tingkat, skala tipografi/jarak, komponen, mekanisme tema); belum ada perubahan kode | – | – |
| 2026-11-22 | FE0–FE3 dikerjakan: token warna, `TingkatHotspot` (kelas LAPAN 30/80), `Navigasi`, `PesanUi`, hak akses per peran di `AppSession`, Dashboard dipecah 6 partial class, bilah status di luar peta, ComboBox lokasi, kolom waktu & wilayah, urut numerik, tab ringkasan per wilayah, panel Wilayah Saya + banner warga, data historis, halaman Daftar/Panduan baru, mode Tamu, Detail dengan verifikasi & navigasi, Peringatan bisa membuka titik panas, teks diseragamkan (glosarium), teks ≥ 12 px. Uji logika lulus; belum diuji di Windows | – | belum di-commit |
