# Akun Uji & Pengujian Peran

> Status: **siap dipakai** (Fase 3, 2026-11-22). Login aplikasi kini lewat tabel `users`; akun uji belum dibuat di DB lokal.
> **Dokumen ini tidak memuat password.** Password akun uji dibuat lokal, tidak pernah ditulis di dokumen, kode, atau commit.

## 1. Tujuan

Menyediakan akun uji per peran dan skenario login/otorisasi untuk fase Auth (F3), tanpa menyimpan kredensial nyata di repo.

## 2. Kondisi saat ini

- Tabel `users` ada (`user_id`, `username` unik, `password_hash`, `role` ∈ Admin/Analis/Operator); masih kosong.
- Login aplikasi mencari akun di tabel `users` dan memverifikasi hash BCrypt. Akun demo lama sudah dihapus dari kode (BUG-10). **Tanpa akun di tabel `users`, tidak ada yang bisa login.**

## 3. Akun uji (hanya username)

| Username uji | Peran | Tujuan |
|---|---|---|
| `admin_uji` | Admin | Uji hak penuh |
| `analis_uji` | Analis | Uji sync, filter, export |
| `operator_uji` | Operator | Uji hak terbatas (lihat/filter/detail) |
| `tidak_ada_uji` | – | **Tidak dibuat**; dipakai untuk uji username tak dikenal |

Username bersifat contoh; tim dapat menggantinya selama memakai awalan `*_uji` agar mudah dibersihkan.

## 4. Cara membuat akun uji (tanpa menulis password)

Hash dibuat langsung oleh PostgreSQL (ekstensi `pgcrypto`, BCrypt cost 11) sehingga tidak perlu alat tambahan; aplikasi memverifikasinya dengan BCrypt.Net (format `$2a$` kompatibel, sudah diuji).

1. Salin templat (berkas salinan sudah di-gitignore):
   ```bash
   cp database/seed-local.example.sql database/seed-local.sql
   ```
2. Buka `database/seed-local.sql`, ganti `<password-admin>`, `<password-analis>`, `<password-operator>` dengan password uji pilihanmu (minimal 8 karakter). **Simpan password hanya di pengelola password**, bukan di repo. Seed otomatis dibatalkan bila penanda belum diganti.
3. Jalankan sebagai `postgres` (membuat ekstensi `pgcrypto` butuh hak pemilik):
   ```bash
   cat database/seed-local.sql | sudo -u postgres psql -v ON_ERROR_STOP=1 -d pyrosnap
   ```
4. Simpan `database/seed-local.sql` hanya di mesin lokal (sudah di-gitignore) sebagai catatan password uji; jangan dibagikan lewat chat/email.
5. Bersihkan akun uji bila perlu: `DELETE FROM users WHERE username LIKE '%\_uji';`

## 5. Peran & matriks hak akses

### 5.1 Peran (keputusan K9, 2026-11-22)

| Peran | Siapa | Ringkas |
|---|---|---|
| **Admin** | Pengelola sistem + analis data (gabungan Admin & Analis lama, karena hak aksesnya sama) | Mengurus sistem dan data: sinkron FIRMS, kelola akun, semua laporan |
| **Operator** | Posko dan petugas lapangan | Menindaklanjuti: alert operasional, export, verifikasi lapangan, navigasi, data historis |
| **Publik** | Warga dengan akun (daftar mandiri, K10) | Mendapat informasi: peta, ringkasan risiko, peringatan berbahasa awam untuk wilayah pilihan, panduan & kontak darurat; boleh ekspor |
| **Tamu** | Warga tanpa akun (tombol "Masuk sebagai tamu", K10) | Sama seperti Publik tetapi **tanpa ekspor** dan wilayah pilihan hanya untuk sesi itu; tidak ada baris di tabel `users` |

### 5.2 Matriks target (diputuskan 2026-11-22, K9 + K10; ⏳ = fitur belum dibuat)

| Aksi | Admin | Operator | Publik | Tamu |
|---|:-:|:-:|:-:|:-:|
| Login/logout | ✅ | ✅ | ✅ | ✅ (masuk sebagai tamu / keluar) |
| Lihat peta, daftar, detail hotspot | ✅ | ✅ | ✅ | ✅ |
| Filter & cari lokasi | ✅ | ✅ | ✅ | ✅ |
| Ringkasan risiko per wilayah ⏳ | ✅ | ✅ | ✅ | ✅ |
| Panduan & kontak darurat ⏳ | ✅ | ✅ | ✅ | ✅ |
| Melihat status verifikasi lapangan ⏳ | ✅ | ✅ | ✅ | ✅ |
| Peringatan berbahasa awam untuk wilayah pilihan ⏳ | – | – | ✅ | ✅ (hanya sesi) |
| Sinkronisasi / muat ulang dari NASA FIRMS | ✅ | ❌ | ❌ | ❌ |
| Alert operasional "segera penanggulangan" (conf ≥ 80) | ✅ | ✅ | ❌ | ❌ |
| Export laporan | ✅ | ✅ | ✅ (K10, akun terdaftar) | ❌ (K10) |
| Verifikasi lapangan (tandai status hotspot) ⏳ | ✅ | ✅ | ❌ | ❌ |
| Navigasi ke titik (salin koordinat, buka arah) ⏳ | ✅ | ✅ | ❌ | ❌ |
| Data historis (pilih rentang tanggal) ⏳ | ✅ | ✅ | ❌ (24 jam terakhir) | ❌ (24 jam terakhir) |
| Kelola akun ⏳ | ✅ | ❌ | ❌ | ❌ |
| Lihat laporan semua user ⏳ | ✅ | ❌ | ❌ | ❌ |

**Kondisi kode per 2026-11-22 (sebelum F7):** peran di database masih `Admin`/`Analis`/`Operator`; Admin & Analis boleh sinkron; semua peran boleh export dan menerima alert. Perubahan ke matriks di atas dikerjakan di Fase 7 (`docs/progress/backend-progress.md`, ROLE-xx/OPS-xx/PUB-xx). Keputusan K6 (Operator boleh export) tetap berlaku; K10: Publik terdaftar boleh ekspor, Tamu tidak; keputusan K5 bagian penerima ("semua user") **diganti**: alert operasional hanya untuk Admin & Operator, Publik mendapat peringatan berbahasa awam.

## 6. Skenario uji

| ID | Skenario | Langkah | Hasil diharapkan |
|---|---|---|---|
| ACC-01 | Akun uji dibuat | Jalankan seed lokal | 3 baris di `users` dengan `password_hash` (bukan teks asli) |
| ACC-02 | Login benar per peran | Login tiap akun uji | Masuk dashboard; tombol sesuai matriks |
| ACC-03 | Password salah | Login `admin_uji` dengan password salah | Pesan generik "username atau password salah" |
| ACC-04 | Username tak dikenal | Login `tidak_ada_uji` | Pesan identik dengan ACC-03 (tak membocorkan) |
| ACC-05 | Kolom kosong | Kosongkan username/password | Validasi input, bukan crash |
| ACC-06 | Hak Operator | Login `operator_uji` | Tombol Sync/Export nonaktif atau disembunyikan |
| ACC-07 | Hak Analis | Login `analis_uji` | Sync & Export aktif; kelola akun tidak ada |
| ACC-08 | Logout | Logout lalu tekan Kembali | Tidak bisa kembali ke dashboard tanpa login; sesi bersih |
| ACC-09 | Akun lama hardcoded | Coba kredensial demo lama | Ditolak (BUG-10 terselesaikan) |
| ACC-10 | Hash tersimpan benar | Cek kolom `password_hash` | Berformat hash bcrypt; tidak ada plaintext |
| ACC-12 | Daftar akun warga | Masuk → Daftar Akun Warga → isi username/password valid | Langsung masuk sebagai Publik; header "nama · Publik"; panel Wilayah Saya tampil |
| ACC-13 | Daftar tidak valid | Username < 3 karakter / password < 8 / konfirmasi beda / username sudah dipakai | Pesan jelas di bilah pesan, akun tidak dibuat |
| ACC-14 | Hak Publik | Login akun Publik | Tanpa tombol Perbarui Data, 🔔 Peringatan, Data Historis; Ekspor ada; Detail tanpa form verifikasi & navigasi |
| ACC-15 | Masuk sebagai Tamu | Masuk → Masuk sebagai Tamu | Header "Tamu"; tanpa Ekspor; wilayah pilihan hanya untuk sesi; Keluar kembali ke halaman Masuk |
| ACC-16 | Admin lama Analis | Setelah migrasi 008, login `analis_uji` | Berperan Admin (bisa Perbarui Data) |
| ACC-17 | Peringatan hanya petugas | Admin memperbarui data, lalu login Publik | Publik tidak punya tombol 🔔; banner warga sesuai wilayah pilihan |
| ACC-11 | Percobaan berulang | 5× salah berturut-turut | (Opsional) jeda/penguncian; keputusan terbuka |

## 7. Checklist target

- [x] **ACC-A1** — Putuskan matriks hak akses §5 (khususnya Export untuk Operator).
- [x] **ACC-A2** — `database/seed-local.sql` ditambahkan ke `.gitignore`.
- [x] **ACC-A3** — Akun uji dibuat lokal dan lolos ACC-01. *(2026-11-22: tiga akun dibuat di DB lokal; password acak hanya di `database/seed-local.sql` yang di-gitignore.)*
- [ ] **ACC-A4** — ACC-02…ACC-10 dijalankan dan lulus.
- [ ] **ACC-A5** — Pembersihan akun uji terdokumentasi dan dicoba.
- [x] **ACC-A6** — Pastikan tidak ada password/hash nyata ter-commit (`git grep`).

## 8. Keputusan terbuka
1. Apakah ada akun Admin awal untuk produksi, dan siapa yang membuat (hanya lewat skrip DB)?
2. Apakah perlu penguncian setelah percobaan gagal berulang (ACC-11)? Ini tambahan di luar diagram.
3. *(Diputuskan 2026-11-22, K10)* Warga lewat dua jalur: **daftar mandiri** di aplikasi (peran Publik, boleh ekspor) dan **masuk sebagai tamu** (tanpa akun, tanpa ekspor). Akun Admin/Operator tetap dibuat lewat skrip DB (atau Kelola Akun bila SCR-14 disetujui).
4. Akun uji setelah F7: `analis_uji` diubah menjadi Admin (atau dihapus) dan `publik_uji` ditambahkan (ROLE-05).

## 9. Rujukan
`database/001_schema.sql` (tabel `users`), `docs/backend/auth-and-roles.md`, `docs/debug/testing-schema.md`, `Claude.md` §4.6, §5
