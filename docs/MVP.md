# MVP — Minimum Viable Product PyroSnap

> Status: **rencana** (dicatat 2026-11-22; diperbarui 2026-11-23 setelah membaca **Modul 10 — Minimum Viable Product**, `docs/MODUL 10 - FIX.pdf`). Bagian "Kondisi saat ini" dan "Celah" berasal dari pemeriksaan repo.
> Rincian status harian ada di `docs/progress/backend-progress.md` dan `docs/progress/frontend-progress.md`; sisa pekerjaan lintas sesi ada di `docs/to-do-list.md`.

## 1. Definisi MVP (Tugas Kelompok Modul 10)

Bunyi tugas (diringkas dari halaman "Tugas Kelompok" Modul 10), **berdasarkan domain model yang sudah dikembangkan**:
1. **Finalisasi** — aplikasi bisa dijalankan dengan baik dan benar.
2. **Layanan web API** — ditambahkan untuk memperoleh nilai tambah. Dalam konteks modul (bab *Mengakses Layanan Pihak Ketiga*, Lab 10.1 *Terhubung dengan REST Services*) artinya **mengonsumsi REST API pihak ketiga**.
3. **Panduan sederhana** — bagaimana aplikasi digunakan.
4. **Pengemasan** — proses instalasi dan konfigurasi dibuat sederhana.
5. **Pengumpulan** — hasil aplikasi (misalnya `.exe`, panduan singkat konfigurasi basis data, dan yang lain) dikumpulkan ke **Worksheet Homework Modul 10 – Minimum Viable Product**.
6. **Pengujian** — aplikasi diuji dengan **framework Black Box Testing** seperti contoh **Lab 10.2**.

Di luar MVP kecuali tim memutuskan lain: Fase 6 (MVVM/DI/unit test formal, `ARCH-B*`), skor risiko (ADD-02), ekspor PDF/XLSX (ADD-05), auto-refresh (ADD-06), Kelola Akun & Riwayat Laporan Admin (SCR-14/15). Air quality (ADD-01) kini **masuk MVP** lewat layanan web API (butir 2).

## 2. Kondisi saat ini (ringkas)

| Butir MVP | Kondisi | Catatan |
|---|---|---|
| Finalisasi | 🟨 Kode lengkap, build bersih (0 error), uji logika di Linux lulus. **Belum pernah dijalankan/diuji di Windows** | UI WPF hanya bisa dijalankan di Windows |
| Web API pihak ketiga | 🟨 Diputuskan 2026-11-23: kualitas udara + cuaca/angin (Open-Meteo) + nama wilayah (Nominatim/OSM) dengan RestSharp; rencana di `docs/backend/web-api.md`; **belum ada kode** | NASA FIRMS sudah dikonsumsi sejak awal |
| Panduan | 🟨 Ada halaman Panduan & Kontak Darurat (untuk warga) dan README singkat. **Belum ada panduan pemakaian aplikasi** | – |
| Pengemasan | ⬜ Belum ada | Lihat celah di bagian 6 |
| Pengujian Black Box | 🟨 Worksheet format Lab 10.2 siap: `docs/debug/blackbox-testing.md` (64 kasus). **Belum dijalankan** | Butuh Windows |
| Pengumpulan | ⬜ Belum | Tujuan: Worksheet Homework Modul 10 |

## 3. Butir 1 — Finalisasi (aplikasi berjalan tanpa error/bug)

**Definisi selesai:** semua alur di `docs/ui/ux/user-flows.md` dan skenario `ACC-01…ACC-17` berjalan di Windows tanpa crash, tanpa pesan error yang tidak terduga, dan setiap kondisi gagal (FLOW-05) memberi pesan yang jelas; build **Release** bersih.

- [ ] **MVP-01** — Jalankan migrasi `008` dan `009` pada database yang dipakai (DB lokal per 2026-11-22 belum memilikinya; `hotspot_verifikasi` belum ada, peran masih `Analis`).
- [ ] **MVP-02** — Uji seluruh aplikasi di Windows per peran (Admin, Operator, Publik, Tamu) memakai daftar di `acount-testing.md` §6 dan `testing-schema.md`; catat hasil di `testing-schema.md` §8.
- [ ] **MVP-03** — Perbaiki semua temuan MVP-02 (bug fungsi lebih dulu, lalu UX). Setiap perbaikan dicatat di `debug/bug-fix.md` dan dipindah ke "Bug yang Diselesasikan" setelah terverifikasi.
- [ ] **MVP-04** — Rotasi key NASA FIRMS (key lama masih ada di riwayat git) dan putuskan soal key CARTO; key baru hanya di `.env`.
- [ ] **MVP-05** — Putuskan Leaflet tetap dari CDN atau dibundel lokal (FT-03d). Dengan CDN, peta kosong tanpa internet.
- [ ] **MVP-06** — Bersihkan kode dan struktur: `ARCH-01…04` (pindahkan `User`/`Alert`/`HotspotReport` ke `Models/`, seragamkan namespace, hapus `MapViewer.cs` bila diputuskan, hapus folder `src/` di root).
- [ ] **MVP-07** — Sinkronkan centang checklist yang masih tertinggal di dokumen (FT-02d, FT-03a, FT-04a, FT-04b sudah dikerjakan di kode tetapi belum dicentang) dan lengkapi dokumen yang kosong (`progress/testing-progress.md`, `report/*`).
- [ ] **MVP-08** — Tinjau teks UI dan isi Panduan bersama tim (CPY-01, CPY-02, CPY-03); isi kontak darurat wajib dicek anggota tim.
- [ ] **MVP-09** — Build Release dan uji ulang alur utama dari hasil build (bukan dari mode debug).

## 4. Butir 2 — Layanan Web API pihak ketiga

Rincian lengkap: **`docs/backend/web-api.md`**.

**Arti di Modul 10:** aplikasi memanggil REST API pihak ketiga (contoh Lab 10.1: RajaOngkir, kelas statis `Ongkir.cs`, pustaka RestSharp). Usulan versi awal dokumen ini untuk membangun server API sendiri (ASP.NET Core) **dibatalkan** karena tidak sesuai konteks modul.

**Keputusan 2026-11-23:**
| Layanan | Penyedia | Nilai tambah |
|---|---|---|
| Kualitas udara (PM2.5, PM10, indeks) | Open-Meteo Air Quality API (data CAMS), tanpa key | Sesuai tujuan README (*air quality*); warga & petugas melihat dampak asap |
| Cuaca & arah angin, kelembapan | Open-Meteo Forecast API, tanpa key | Perkiraan arah sebaran asap dan kondisi kering di sekitar titik |
| Nama wilayah dari koordinat | Nominatim (OpenStreetMap), tanpa key | Nama desa/kabupaten/provinsi nyata (menggantikan 8 area pantau sementara, K2) |

Pustaka: **RestSharp** seperti Lab 10.1. Ketentuan penting: atribusi Open-Meteo (CC BY 4.0) dan OpenStreetMap; tingkat gratis Open-Meteo hanya non-komersial (< 10.000 panggilan/hari); Nominatim maksimal 1 permintaan/detik untuk seluruh aplikasi, User-Agent identitas aplikasi, wajib cache, tanpa permintaan massal.

- [x] **MVP-10** — Tentukan layanan yang masuk MVP (2026-11-23: ketiga layanan di atas).
- [x] **MVP-11** — Tentukan teknologi: RestSharp, kelas layanan statis di `Services/` seperti pola Lab 10.1.
- [x] **MVP-12** — RestSharp **112.1.0** (106.11.4 saran modul punya celah CVE-2021-27293) — WEB-00, 2026-11-23.
- [x] **MVP-13** — Rencana kontrak & penempatan di aplikasi: `docs/backend/web-api.md`.
- [x] **MVP-14** — Implementasi + uji otomatis (WEB-01…08) selesai 2026-11-23, 25/25 lulus termasuk panggilan asli. Sisa: bagian F worksheet Black Box di Windows (WEB-09).
- [ ] **MVP-15** — Jelaskan data tambahan dan sumbernya di panduan pemakaian (WEB-10).

## 5. Butir 3 — Panduan sederhana penggunaan

Target: satu dokumen pendek (usulan `docs/panduan-pengguna.md`, juga ikut di paket pengumpulan), bahasa awam, dengan tangkapan layar.

- [ ] **MVP-16** — Susun panduan: apa itu PyroSnap; cara menjalankan; cara masuk (akun, daftar warga, tamu); tugas utama per peran (Admin, Operator, Publik, Tamu); cara membaca peta, tingkat keyakinan, dan peringatan; ekspor laporan; arti istilah; pemecahan masalah umum (database tidak tersedia, key FIRMS belum diatur, peta kosong tanpa internet).
- [ ] **MVP-17** — Ambil tangkapan layar dari hasil uji Windows (setelah MVP-02/03).
- [ ] **MVP-18** — Uji keterbacaan: minta 1–2 orang di luar tim mengikuti panduan dari nol (FLOW-A3).

## 6. Butir 4 — Pengemasan (instalasi & konfigurasi sederhana)

**Celah yang ditemukan saat ini:**
- Aplikasi hanya bisa dijalankan dari kode (`dotnet run`); belum ada `.exe` terbit.
- **Skrip database memakai bash dan `sudo -u postgres`** (`migrate.sh`, cara seed akun uji); itu **tidak berjalan di Windows**. Perlu cara yang Windows-friendly (mis. satu berkas SQL gabungan + skrip PowerShell/`.bat`).
- Konfigurasi lewat berkas `.env` yang dibuat manual; **belum ada `.env.example`**. `AppConfig` mencari `.env` dari folder aplikasi naik hingga 5 tingkat, jadi untuk aplikasi terpasang `.env` diletakkan di samping `.exe`.
- Peta butuh **WebView2 Runtime**; tanpanya jatuh ke peta kanvas sederhana. Pemasang perlu memeriksa atau memasangnya.
- Akun awal (Admin) belum ada cara pembuatan yang sederhana; sekarang lewat skrip SQL dengan `pgcrypto`.
- Setiap pengguna butuh key NASA FIRMS sendiri; key **tidak boleh** dibundel ke dalam paket.

Pilihan bentuk kemasan (diputuskan tim):

| Opsi | Isi | Catatan |
|---|---|---|
| A. Publish mandiri | `dotnet publish` (win-x64, self-contained) lalu dikemas sebagai `.zip` + skrip konfigurasi | Paling sederhana; tidak butuh .NET terpasang |
| B. Installer | Inno Setup atau MSIX | Lebih rapi untuk pengguna awam; lebih banyak pekerjaan |

Pekerjaan:
> **Database online (2026-11-23):** Supabase sudah disiapkan agar tester cukup memasang `.exe`; kredensial dan risikonya di `docs/database/supabase.md`. Paket hanya boleh memuat koneksi `pyrosnap_app`, tidak pernah `SUPABASE_ADMIN_DB`.

- [x] **MVP-19** — Opsi A: `dotnet publish` win-x64 self-contained, dikemas ZIP + `BACA-DULU.txt`; database online Supabase.
- [ ] **MVP-20** — `.exe` Release win-x64 sudah dibuat dan lulus tes awal di Windows tim (2026-11-23). **Sisa:** uji di komputer Windows **bersih** (tanpa SDK, tanpa Visual Studio), karena komputer tim sudah memasang .NET SDK.
- [ ] **MVP-21** — Buat `.env.example` (tanpa nilai rahasia) dan cara pembuatan `.env` yang mudah (mis. skrip yang menanyakan nilai, atau berkas contoh yang tinggal diisi).
- [ ] **MVP-22** — Skrip database untuk Windows: pembuatan database + semua migrasi (`000`–`009`) + seed sumber data, tanpa bash; opsional satu berkas SQL gabungan.
- [ ] **MVP-23** — Cara membuat akun Admin pertama yang sederhana (skrip, tanpa menulis password di berkas yang di-commit).
- [ ] **MVP-24** — Periksa/pasang WebView2 Runtime dalam alur instalasi, atau jelaskan di panduan.
- [ ] **MVP-25** — Tulis **panduan singkat konfigurasi basis data** (`docs/database/setup-and-seed.md` diringkas untuk paket pengumpulan).
- [ ] **MVP-26** — Uji instalasi dari nol mengikuti panduan di mesin lain.

## 7. Butir 6 — Pengujian Black Box (format Lab 10.2 Modul 10)

**Format dari Modul 10 (sudah diketahui):** worksheet berisi *Nama Aplikasi*, *Tanggal Pengujian*, *Tester*; kolom **No · Halaman yang Diuji · Aksi Aktor · Reaksi Sistem (Benar / Salah) · Hasil (Sesuai / Tidak Sesuai)**, dikelompokkan per jenis pengguna (contoh modul: Halaman Sistem Admin, Halaman Sistem Pelanggan). Teknik: **functional testing, black box**. Siklus menurut modul: identifikasi → perancangan test case → persiapan → eksekusi → membandingkan (gagal → perbaiki → uji ulang).

Bahan yang sudah ada:
- **Worksheet siap pakai: `docs/debug/blackbox-testing.md`** — 64 kasus: A Umum (14), B Admin (17), C Operator (7), D Publik (4), E Tamu (3), F Web API (7), G Instalasi (4, menunggu kemasan), H Tampilan & Tema (8; No. 1–5 dijalankan di kedua tema).
- Uji logika di Linux (unit/integration, program uji sementara, **tidak di repo**): parser 30/30, repository F4 26/26, alert 23/23, F7 37/37, matriks peran 7/7, peta 7/7 & 10/10. Modul 10 tidak mewajibkan uji otomatis; yang wajib adalah worksheet black box.

- [x] **MVP-27** — Dapatkan format Modul 10 (2026-11-23: `docs/MODUL 10 - FIX.pdf`).
- [x] **MVP-28** — Petakan skenario ke format Lab 10.2 → `docs/debug/blackbox-testing.md`.
- [ ] **MVP-29** — (Opsional) jadikan uji logika sementara sebagai proyek uji resmi; tidak diwajibkan Modul 10.
- [ ] **MVP-30** — Jalankan worksheet di Windows pada hasil build Release (isi Tanggal, Tester, Hasil); ulangi kasus yang gagal setelah diperbaiki.
- [ ] **MVP-31** — Salin hasil ke Worksheet Homework Modul 10 dan rangkum di `docs/progress/testing-progress.md`.

## 8. Butir 5 — Pengumpulan (Worksheet Homework Modul 10 – Minimum Viable Product)

Modul 10 menyebut isi pengumpulan "misalnya dalam bentuk .exe, panduan singkat konfigurasi basis data dan yang lain", ditambah hasil uji Black Box. Usulan isi:
- [ ] **MVP-32** — Aplikasi dalam bentuk `.exe` (hasil publish Release), siap dijalankan.
- [ ] **MVP-33** — Panduan singkat konfigurasi basis data + skrip database untuk Windows (MVP-22, MVP-25).
- [ ] **MVP-34** — Panduan pemakaian (MVP-16) dan `.env.example` (MVP-21).
- [ ] **MVP-35** — Worksheet Black Box yang sudah diisi (MVP-30/31).
- [ ] **MVP-36** — Berkas lain bila diminta dosen/asisten (mis. tautan repo, laporan akhir `docs/report/final-report.md`, diagram domain model). Bentuk "Worksheet Homework" (dokumen/formulir) perlu dicek di kelas.
- [ ] **MVP-37** — Periksa paket: tidak ada `.env`, `seed-local.sql`, key, atau password; coba pasang dari paket yang akan dikumpulkan.

## 9. Urutan kerja yang disarankan

1. MVP-01 → MVP-04 (migrasi DB 008/009, uji Windows, rotasi key), karena semuanya bergantung pada aplikasi yang terbukti jalan.
2. Layanan web API (MVP-12, MVP-14) — **sudah dikodekan**; tinggal uji bagian F di Windows.
3. Pengemasan (MVP-19…26) setelah aplikasi dan web API stabil.
4. Jalankan worksheet Black Box (MVP-30) pada hasil kemasan, perbaiki yang gagal, uji ulang.
5. Panduan pemakaian (MVP-16…18) dengan tangkapan layar dari hasil uji.
6. Sistem desain (FE4, `docs/ui/design-system.md`): gaya D5 dan tema terang/gelap (DS-04…09) sudah diterapkan di kode; tetap .NET 8 LTS. Sisa DS-10: cek di layar Windows untuk **kedua tema** — ini melipatgandakan pekerjaan uji black-box (jalankan kasus tampilan di terang dan gelap).
7. Pengumpulan ke Worksheet Homework (MVP-32…37).

## 10. Keputusan terbuka (MVP)
1. ~~Versi RestSharp~~ — diputuskan 112.1.0.
2. Nama wilayah dari Nominatim disimpan ke database atau cukup cache (WEB-11, K2).
3. Bentuk kemasan: zip + skrip atau installer (MVP-19).
4. Bentuk pasti Worksheet Homework dan berkas tambahan (MVP-36).
5. *(Diputuskan 2026-11-23)* `docs/MODUL 10 - FIX.pdf` **tidak** di-commit; sudah dimasukkan ke `.gitignore` (berisi materi kuliah dan API key praktikum).

## 11. Rujukan
`docs/MODUL 10 - FIX.pdf`, `docs/backend/web-api.md`, `docs/debug/blackbox-testing.md`, `docs/to-do-list.md`, `docs/progress/backend-progress.md`, `docs/progress/frontend-progress.md`, `docs/database/setup-and-seed.md`, `docs/database/acount-testing.md`, `docs/ui/ux/user-flows.md`, `log.md`

