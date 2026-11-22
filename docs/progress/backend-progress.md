# Progres Backend, Database & Debug PyroSnap

> Papan pelacakan. Diperbarui manual setiap ada perubahan. Dibuat 2026-11-22.
> Lihat §4 untuk riwayat perubahan per fase.
> Frontend/UI dan laporan akhir punya papan sendiri (belum diisi).

## 1. Ringkasan fase

| Fase | Nama | Prioritas | Status | Dokumen utama |
|---|---|---|---|---|
| F0 | Keamanan & konfigurasi | P0 | 🟨 kode selesai; rotate key & uji Windows menunggu | `debug/bug-fix.md` (BUG-01/02), `backend/data-source-api.md` (BE-00) |
| F1 | Data pipeline & bug fungsi | P0–P1 | 🟨 kode selesai & uji logika lulus; uji Windows menunggu | `backend/data-source-api.md`, `debug/bug-fix.md` |
| F2 | Fondasi database | P1 | 🟨 migrasi DB lokal selesai & terverifikasi; uji Windows & anggota lain menunggu | `architecture/database-architecture.md`, `database/schema-changes.md`, `database/setup-and-seed.md` |
| F3 | Autentikasi & peran | P1 | 🟨 kode selesai & uji logika lulus; akun uji & uji Windows menunggu | `backend/auth-and-roles.md`, `database/acount-testing.md` |
| F4 | Persistensi hotspot & laporan | P2 | 🟨 kode selesai & uji repository lulus; uji Windows menunggu | `architecture/database-architecture.md`, `backend/report-and-alert.md` |
| F5 | Alert | P2 | 🟨 kode selesai & uji repository lulus; uji Windows menunggu | `backend/report-and-alert.md` (Bagian B) |
| F6 | Opsional: service+interface, MVVM, DI, unit test | – | ⬜ menunggu persetujuan | `architecture/backend-architecture.md` (ARCH-B*) |
| F7 | Peran Admin/Operator/Publik + fitur warga & lapangan | P1 | 🟨 kode & migrasi 008/009 selesai, teruji di tabel sementara (37/37); migrasi DB lokal & uji Windows menunggu | `database/acount-testing.md` §5, `feature/additional-feature.md` (ADD-09…13) |

Legenda: ⬜ belum mulai · 🟨 berjalan · ✅ selesai & terverifikasi · ⛔ terblokir.

## 2. Checklist per fase

### F0 — Keamanan & konfigurasi
- [x] BE-00 / BUG-01 — Key FIRMS keluar dari kode
- [ ] BE-00 / BUG-01 — Key FIRMS lama dirotate (pemilik akun NASA)
- [ ] CARTO — Putuskan apakah key CARTO lama perlu dirotate (ada di riwayat git)
- [x] BUG-02 — Key CARTO keluar dari `map.html`
- [x] ARCH-05 — `AppConfig` (env + `.env`)
- [x] DBA-01 — `PYROSNAP_DB` dibaca dari `.env` (BUG-12)
- [x] TST-08 — Pencarian key lama di working tree: nol temuan (riwayat git masih memuatnya)
- [ ] Uji di Windows: data FIRMS termuat dengan key dari `.env`, tile peta tampil, pesan muncul bila key kosong

### F1 — Data pipeline & bug fungsi
- [x] BE-01 — Skema kolom CSV dikonfirmasi dari data asli (TST-01)
- [x] BE-02…BE-05 — Parser: header, confidence, waktu UTC, TryParse (BUG-03/04/06)
- [x] BE-06 — Fallback dummy dihapus (BUG-05)
- [x] BE-07, BE-09, BE-10 — Timeout, hasil sync bermakna, pesan error (BUG-15)
- [x] BE-08 — URL/key tidak ada di view
- [x] BUG-19 — Sumber default diganti `VIIRS_NOAA20_NRT` (SNPP berhenti 2026-11-02)
- [x] BUG-07 / RPT-02 — Format angka InvariantCulture
- [x] BUG-08 / ARCH-07 — State dashboard bertahan saat navigasi
- [x] BUG-13 / ARCH-06 — Filter komposable (area masih terpisah dari `Wilayah`, menunggu K2)
- [x] BUG-14, BUG-16 — Peta tidak melompat + sorot titik terpilih; error export ditangani
- [x] RPT-01, RPT-03 — Teks laporan satu sumber; I/O aman
- [x] Uji logika (program uji sementara, 30/30 lulus): TST-01…TST-06, TST-14, TST-15 level model, sync live & key salah
- [ ] Uji di Windows: TST-04 (tampilan), TST-07, TST-10…TST-13, TST-15 (Excel), TST-16

### F2 — Fondasi database
- [x] SET-03…SET-06 — Seed idempoten, `.gitignore`, README, rename berkas
- [ ] SET-01, SET-02 — Versi PostgreSQL disepakati; setup dicoba 2 anggota
- [x] SET-07 — Migrasi `002`–`007` dijalankan pada DB lokal sebagai `postgres` (diverifikasi: constraint, TIMESTAMPTZ, seed)
- [x] SCH-A1 — Paket inti disetujui (SCH-01/02/03/06/07/08), SCH-05 ditunda; SCH-A2 opsi A
- [x] DBA-03 — Skrip migrasi bernomor + `migrate.sh`; DBA-10 rename berkas janggal
- [x] DBA-02 — Status koneksi DB di halaman Login (latar belakang, tak crash saat mati)
- [x] BUG-11 — Format laporan TXT diterima (`004`)
- [x] Uji TST-20…TST-24, TST-26 (tabel sementara PostgreSQL 18.6) dan TST-27/28 (logika) lulus
- [ ] Uji di Windows: status DB tampil di halaman Login

### F3 — Autentikasi & peran
- [x] ACC-A1 — Matriks hak akses diputuskan (K6: Operator boleh export, tidak boleh sync)
- [x] AUTH-01…AUTH-09 — Login via `users` (BCrypt), peran mengatur tombol Muat Ulang, logout bersih, riwayat navigasi dihapus
- [x] ACC-A2, ACC-A6 — `seed-local.sql` di-gitignore; templat `database/seed-local.example.sql` tanpa password
- [x] ACC-A3 — Akun uji `admin_uji`, `analis_uji`, `operator_uji` dibuat di DB lokal (login diverifikasi lewat `UserRepository` + `User.Login`)
- [ ] ACC-A4, ACC-A5 — Skenario ACC-02…ACC-10 dicoba di aplikasi (Windows)
- [x] BUG-10 — Kredensial hardcoded hilang (akun demo lama tidak bisa login lagi)
- [ ] Uji TST-30…TST-33, ACC-01…ACC-10 lulus

### F4 — Persistensi hotspot & laporan
- [x] DBA-04, DBA-05 — Upsert (unnest + ON CONFLICT) dan query filter setara `IsMatch` (BE-11, FT-08a)
- [x] Alur data: Admin/Analis sinkron FIRMS → simpan DB (gagal FIRMS → data DB terakhir); Operator baca DB saja
- [x] DBA-07 / RPT-04, RPT-05 — Laporan tercatat (user, format, waktu, file) + tautan hotspot dalam satu transaksi
- [x] DBA-09 — Audit: semua query berparameter; tidak ada interpolasi input ke SQL
- [x] Uji repository 26/26 lulus (TST-23, TST-25, agregasi laporan) di tabel sementara dengan data FIRMS asli
- [ ] Uji di Windows: sinkron Admin menyimpan ke DB, Operator membaca dari DB, export tercatat

### F5 — Alert
- [x] ALR-01…ALR-07 — Ambang LAPAN conf ≥ 80, alert ke semua user, maks 50 per sinkron, tanpa duplikat, halaman Alert + badge
- [x] DBA-08 — `AlertRepository` (kirim, daftar, jumlah belum dibaca, tandai dibaca)
- [x] Uji TST-34…TST-36 (23/23) di tabel sementara dengan data FIRMS asli
- [ ] Uji di Windows: sinkron Admin memunculkan badge alert; Operator melihat alert yang sama; tandai dibaca

### F6 — Opsional (hanya setelah disetujui)
- [ ] ARCH-B1…B4, AUTH-B1…B3, ALR-B1, RPT-B1, TST-B1

### F7 — Peran Admin/Operator/Publik + fitur warga & lapangan (disetujui 2026-11-22, K9)
Peran & hak akses (backend/database):
- [x] **ROLE-01** — Migrasi `008`: akun `Analis` → `Admin`; CHECK `role IN ('Admin','Operator','Publik')`. *Selesai bila* DB lokal termigrasi dan uji skema lulus.
- [x] **ROLE-02** — Hak akses per peran di satu tempat (sinkron: Admin; export, alert operasional, verifikasi, navigasi, data historis: Admin & Operator). *Selesai bila* matriks `acount-testing.md` §5.2 terpenuhi.
- [x] **ROLE-03** — Alert operasional hanya untuk Admin & Operator (mengganti "semua user" pada K5).
- [x] **ROLE-04** — Akun warga (K10): `UserRepository.DaftarPublikAsync` untuk daftar mandiri (peran Publik, hash BCrypt, username unik) + sesi Tamu tanpa baris DB. Hak ekspor: Publik ya, Tamu tidak.
- [ ] **ROLE-05** — Akun uji: `analis_uji` otomatis jadi Admin lewat migrasi 008; akun Publik uji dibuat lewat layar Daftar di aplikasi.

Nilai tambah Operator (posko & lapangan):
- [x] **OPS-01** — Verifikasi lapangan: tabel baru (hotspot, user, status *api aktif / sudah padam / bukan kebakaran*, catatan, waktu); Operator/Admin mengisi di Detail; semua peran melihat status terakhir. (ADD-10)
- [x] **OPS-02** — Navigasi: salin koordinat + buka arah di peta daring dari Detail. (ADD-11)
- [x] **OPS-03** — Data historis: pilih rentang tanggal lewat `FilterCriteria` + `HotspotRepository.QueryAsync`; Publik tetap 24 jam terakhir. (ADD-12)

Informasi untuk warga (Publik):
- [x] **PUB-01** — Wilayah pilihan per user (memakai tabel `filter_criteria.wilayah` yang sudah ada) + peringatan berbahasa awam bila ada hotspot berkeyakinan tinggi di wilayah itu. (ADD-03, butuh K2)
- [x] **PUB-02** — Ringkasan risiko per wilayah: jumlah hotspot dan tingkatnya per provinsi/area hari ini. (ADD-04, butuh K2)
- [x] **PUB-03** — Halaman panduan & kontak darurat; isi dan nomor dari sumber resmi yang diverifikasi tim. (ADD-13)

Uji:
- [x] Skenario per peran ditambahkan ke `database/acount-testing.md` §6 (ACC-12…ACC-17)
- [ ] **ROLE-06** — Jalankan migrasi `008` dan `009` pada DB lokal sebagai `postgres`, lalu jalankan ACC-12…ACC-17 di Windows

### Struktur & kebersihan (lintas fase)
- [ ] ARCH-01…ARCH-04 — Folder, namespace, hapus kode mati dan `src/`
- [ ] ARCH-08, ARCH-09, ARCH-10 — Teks laporan, aturan error, dokumentasi alur

## 3. Keputusan yang masih menunggu

| # | Keputusan | Dokumen | Status |
|---|---|---|---|
| K1 | Nasib `MapViewer.cs` | backend-architecture §5 | ⬜ |
| K2 | Atribut wilayah pada Hotspot / hapus dari filter | backend-architecture §5, FT-02c | 🟨 sementara: 8 area pantau (`Services/WilayahPantau.cs`, ±3° dari pusat, pusat terdekat); batas administratif menunggu ADD-07 |
| K3 | Pemetaan confidence VIIRS (SCH-04) | schema-changes | 🟨 sementara opsi A (l=30, n=60, h=90) dipakai di F1; tim bisa mengubah |
| K4 | Algoritma hash (BCrypt vs PBKDF2 bawaan) | auth-and-roles §5 | ✅ BCrypt (BCrypt.Net-Next 4.0.3; seed via pgcrypto) |
| K5 | Ambang kritis alert dan penerimanya | report-and-alert B.4 | ✅ conf ≥ 80 (kelas tinggi LAPAN); semua user |
| K6 | Matriks hak akses (Operator boleh export?) | acount-testing §5 | ✅ Operator boleh export, tidak boleh sync |
| K7 | Repository class dianggap tambahan atau tidak | backend-architecture §5 | ⬜ |
| K8 | Fitur tambahan (ADD-01…ADD-08) | additional-feature | 🟨 ADD-03/04 sebagian disetujui (untuk Publik); lainnya menunggu |
| K9 | Peran pengguna | acount-testing §5 | ✅ Admin+Analis digabung jadi Admin; tambah Publik; Operator punya alert operasional, export, verifikasi, navigasi, data historis |
| K10 | Cara akun Publik dibuat (ROLE-04) | acount-testing §8 | ✅ daftar mandiri (Publik, boleh ekspor) + masuk sebagai tamu (tanpa ekspor) |

## 4. Log perubahan

| Tanggal | Perubahan | Oleh | Commit |
|---|---|---|---|
| 2026-11-22 | Dokumen perencanaan dibuat (tanpa perubahan kode) | – | – |
| 2026-11-22 | F0: `AppConfig` baru; key FIRMS/CARTO dan `PYROSNAP_DB` dibaca dari env/`.env`; default koneksi tanpa password dihapus. Build Linux (EnableWindowsTargeting) 0 error, warning sama dengan sebelumnya | – | belum di-commit |
| 2026-11-22 | F1: parser FIRMS berbasis header (confidence l/n/h, waktu UTC, TryParse), dummy dihapus, timeout & pesan error, sumber default NOAA-20 (`FIRMS_SOURCE`), state dashboard di `AppSession`, filter komposabel, peta tak melompat + sorot, CSV/TXT dari `HotspotReport` dengan InvariantCulture, error export ditangani. Uji logika 30/30 lulus; build 0 error | – | belum di-commit |
| 2026-11-22 | F2: berkas SQL diganti nama (`000_create_database.sql`, `001_schema.sql`); migrasi `002`–`007` (unik hotspot, TIMESTAMPTZ, format TXT, sumber unik, alert unik, seed NOAA-20) + `migrate.sh`; status DB di halaman Login; `.gitignore` seed lokal; README setup. Uji skema di tabel sementara lulus; DB asli belum dimigrasi | – | belum di-commit |
| 2026-11-22 | F2: migrasi `002`–`007` dijalankan oleh pemilik DB dan diverifikasi | – | – |
| 2026-11-22 | F3: BCrypt.Net-Next 4.0.3; `User` menyimpan hash dan memverifikasi BCrypt; `UserRepository` (query berparameter); login lewat DB dengan pesan generik dan pesan khusus DB mati; akun demo dihapus; peran Admin/Analis untuk Muat Ulang; nama + peran di header; logout membersihkan sesi; riwayat Frame dihapus; templat `seed-local.example.sql` (pgcrypto). Uji logika 13/13 lulus; build 0 error, warning turun 16 → 8 | – | belum di-commit |
| 2026-11-22 | F4: `HotspotRepository` (upsert batch tanpa duplikat, muat 24 jam terakhir, query setara `FilterCriteria.IsMatch`), `ReportRepository` (laporan + tautan hotspot dalam satu transaksi); dashboard: Admin/Analis sinkron FIRMS lalu simpan ke DB, fallback ke DB bila FIRMS gagal, Operator membaca DB saja; export mencatat laporan ke DB (gagal catat tidak membatalkan file). Uji repository 26/26 lulus; build 0 error | – | belum di-commit |
| 2026-11-22 | F7: migrasi `008_peran_publik.sql` (Analis → Admin, CHECK Admin/Operator/Publik) dan `009_hotspot_verifikasi.sql`; `UserRepository.DaftarPublikAsync`, `VerifikasiRepository`, `PreferensiRepository` (wilayah pilihan di `filter_criteria`), `WilayahPantau`; alert operasional hanya Admin & Operator; pemetaan VIIRS `l` diubah 30 → 20 agar masuk kelas Rendah LAPAN. Uji 37/37 di tabel sementara | – | belum di-commit |
| 2026-11-22 | Keputusan K10 (daftar mandiri + tamu) dan rencana frontend & UI/UX dibuat (`docs/frontend/`, `docs/ui/`, `progress/frontend-progress.md`) | – | – |
| 2026-11-22 | Keputusan K9: peran Admin/Operator/Publik, fitur warga & nilai tambah Operator dicatat sebagai Fase 7 (belum ada perubahan kode) | – | – |
| 2026-11-22 | F5: `AmbangKritis` (conf ≥ 80 sesuai kelas tinggi LAPAN, maks 50 alert per sinkron, urut FRP), `Alert.SendAlert()` tanpa konsol, `AlertRepository` (kirim ke semua user tanpa duplikat, daftar, jumlah belum dibaca, tandai dibaca hanya milik sendiri), pemicu setelah sinkron tersimpan, tombol 🔔 Alert (jumlah belum dibaca) + halaman `AlertView`. Uji 23/23 lulus; build 0 error, warning 4 | – | belum di-commit |

## 5. Catatan
- Verifikasi UI WPF memerlukan Windows + WebView2 Runtime.
- Setiap item yang dicentang harus punya bukti uji (ID TST/ACC) dan, bila relevan, dipindah ke "Bug yang Diselesasikan" di `debug/bug-fix.md`.
