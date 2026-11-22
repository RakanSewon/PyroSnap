# Fitur Utama PyroSnap (sesuai class diagram)

> Status dokumen: **rencana**. Kolom "Sekarang" berdasarkan review kode 2026-11-22. Fitur di luar diagram ada di `additional-feature.md`.

## 1. Tujuan

Mendaftar fitur inti yang berasal dari 8 class diagram beserta kondisi saat ini, perilaku target, dan kriteria selesai, supaya jelas mana yang sudah jalan, mana yang semu, dan mana yang belum ada backend-nya.

## 2. Ringkasan status

| ID | Fitur | Class terkait | Sekarang | Fase |
|---|---|---|---|---|
| FT-01 | Sinkronisasi data hotspot | HotspotDataSource | Terhubung ke FIRMS, tapi data keliru (confidence, waktu) dan gagal diam-diam | F1 |
| FT-02 | Filter hotspot | FilterCriteria | Hanya confidence & FRP; tidak komposable | F1/F2 |
| FT-03 | Peta & pin hotspot | MapViewer / Location | Leaflet jalan; sorot pilihan hanya di mode kanvas | F1 |
| FT-04 | Detail hotspot | Hotspot | Ada; label "keparahan" menyesatkan | F1 |
| FT-05 | Report & export | HotspotReport | TXT/CSV saja; CSV rusak di locale id-ID; tidak tercatat di DB | F1, F4 |
| FT-06 | Login & peran | User | Kredensial hardcoded, tanpa DB, peran tidak dipakai | F3 |
| FT-07 | Alert ambang kritis | Alert | Belum pernah dipicu; hanya `Console.WriteLine` | F5 |
| FT-08 | Penyimpanan data (DB) | (semua) | Belum dipakai | F2, F4 |

## 3. Detail fitur & checklist

### FT-01 — Sinkronisasi data hotspot
**Relasi diagram:** `User --mensinkronisasi--> HotspotDataSource`, `Hotspot --berasal dari--> HotspotDataSource`.
**Target:** `SyncAsync()` mengambil CSV FIRMS (bbox Indonesia), mengubahnya menjadi `Hotspot` yang benar, mencatat `WaktuSinkronisasi`, dan melaporkan kegagalan secara jelas.
- [x] **FT-01a** — Parser membaca confidence, brightness, FRP, dan waktu akuisisi (UTC) dengan benar. *Selesai bila* 1 baris CSV asli menghasilkan `Hotspot` yang cocok nilai per nilainya. (BUG-03, BUG-04, BUG-06)
- [x] **FT-01b** — Kegagalan jaringan/parse memunculkan pesan error, bukan data dummy. *Selesai bila* mematikan internet → UI menampilkan "gagal memuat" dan tidak ada titik palsu. (BUG-05)
- [x] **FT-01c** — UI menampilkan sumber dan waktu sinkronisasi terakhir. *Selesai bila* terlihat "NASA FIRMS — sinkron HH:mm".
- [x] **FT-01d** — Hanya peran Admin/Analis yang boleh menekan Sinkron/Muat Ulang (`Claude.md` §5). *Selesai bila* Operator tidak bisa memicu sync. (butuh F3)

### FT-02 — Filter hotspot
**Relasi diagram:** `FilterCriteria --menyaring--> Hotspot`, `User --mengubah--> FilterCriteria`, `MapViewer --menerapkan--> FilterCriteria`.
- [x] **FT-02a** — Satu objek `FilterCriteria` menampung semua kriteria aktif (waktu, confidence, FRP, wilayah). *Selesai bila* mencari lokasi + filter confidence berlaku bersamaan, urutan apa pun. (BUG-13, ARCH-06)
- [x] **FT-02b** — `Reset()` mengembalikan semua kriteria dan daftar ke semula. *Selesai bila* kembali ke jumlah titik awal.
- [ ] **FT-02c** — Keputusan `Wilayah`: dievaluasi (atribut wilayah di Hotspot) atau dihapus dari filter. *Selesai bila* keputusan tercatat dan konsisten di kode, diagram, dan DB.
- [ ] **FT-02d** — Filter rentang waktu tersedia di UI (model sudah mendukung `WaktuMulai`/`WaktuAkhir`).

### FT-03 — Peta & pin hotspot
**Relasi diagram:** `MapViewer --menampilkan pin--> Hotspot`, `Location --menampilkan--> Hotspot`.
- [ ] **FT-03a** — Pin dirender dari daftar hasil filter; warna mengikuti tingkat confidence.
- [x] **FT-03b** — Memilih baris di daftar menyorot dan memusatkan pin pada peta (mode Leaflet). *Selesai bila* terlihat pada peta tanpa memilih manual.
- [x] **FT-03c** — Menerapkan filter tidak mengubah zoom/posisi peta secara paksa. (BUG-14)
- [ ] **FT-03d** — Leaflet dibundel lokal; tanpa internet, peta menampilkan pesan yang jelas.

### FT-04 — Detail hotspot
**Relasi diagram:** `Hotspot.getDetail()`.
- [ ] **FT-04a** — Detail menampilkan koordinat, confidence, brightness, FRP, waktu (UTC dan lokal).
- [ ] **FT-04b** — Label "Tingkat Keparahan" diganti istilah yang tepat (mis. "Keyakinan deteksi"). (catatan UI)
- [x] **FT-04c** — `GetDetail()` memakai `InvariantCulture`. (BUG-07)

### FT-05 — Report & export
**Relasi diagram:** `HotspotReport --berisi--> Hotspot`, `User --mencetak--> HotspotReport`.
- [x] **FT-05a** — Export TXT dan CSV berjalan dari satu sumber teks (`Generate()`). (ARCH-08)
- [x] **FT-05b** — CSV memakai `InvariantCulture` dan aman dibuka di Excel locale id-ID. (BUG-07)
- [x] **FT-05c** — Error penyimpanan (file terkunci/izin) ditangani dengan pesan jelas. (BUG-16)
- [x] **FT-05d** — Tiap export tercatat di `hotspot_report` + `report_hotspot` dengan `user_id` pembuat. (F4, lihat `backend/report-and-alert.md`)
- [x] **FT-05e** — Hanya peran yang berhak yang dapat mengekspor. (F3)

### FT-06 — Login & peran
**Relasi diagram:** `User.login()`, `User.logout()`.
- [x] **FT-06a** — Login memeriksa tabel `users` dengan hash; tanpa kredensial hardcoded. (BUG-10, lihat `backend/auth-and-roles.md`)
- [x] **FT-06b** — Peran Admin/Analis/Operator menentukan tombol yang aktif.
- [x] **FT-06c** — Logout membersihkan sesi (user, data terpilih, filter). 
- [x] **FT-06d** — Pesan gagal login tidak membocorkan apakah username ada.

### FT-07 — Alert ambang kritis
**Relasi diagram:** `Hotspot --memicu--> Notification/Alert` (garis putus-putus).
- [x] **FT-07a** — Ambang kritis (confidence dan/atau FRP) terdefinisi di satu tempat.
- [x] **FT-07b** — Saat sync, hotspot di atas ambang menghasilkan `Alert`; `Hotspot` tidak memanggil `Alert` langsung.
- [x] **FT-07c** — Alert tampil di UI (badge/daftar), bukan `Console.WriteLine`; `MarkAsRead()` berfungsi.
- [x] **FT-07d** — Alert tersimpan di tabel `alert`. (F5)

### FT-08 — Penyimpanan data (DB)
- [x] **FT-08a** — Hotspot hasil sync tersimpan tanpa duplikat. (DBA-04)
- [x] **FT-08b** — Aplikasi tetap berfungsi (mode tanpa DB) saat database tidak tersedia, dengan indikator status. (DBA-02)

## 4. Keputusan terbuka
1. *(Diputuskan 2026-11-22)* Ambang kritis = confidence ≥ 80 (kelas tinggi LAPAN, "segera penanggulangan"); FRP hanya informasi.
2. *(Diputuskan 2026-11-22)* Alert dikirim ke semua user (Admin, Analis, Operator).
3. Apakah Operator boleh melihat laporan yang dibuat user lain?

## 5. Rujukan
`Claude.md` §5, `Diagram_Pyro_Snap.drawio.png`, `docs/debug/bug-fix.md`, `docs/backend/*`, `docs/progress/backend-progress.md`
