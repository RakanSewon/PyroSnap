# Database Online (Supabase)

> Status (2026-11-23): database PyroSnap sudah berjalan di Supabase (PostgreSQL 17, region Singapura). Tujuannya: tester cukup memasang `.exe` tanpa memasang PostgreSQL. Uji repository asli sebagai user aplikasi lulus 41/41 dari Linux; **belum dicoba dari aplikasi di Windows**.
> **Dokumen ini tidak memuat password, connection string, atau alamat proyek.**

## 1. Dua koneksi, dua fungsi

| Nama di `.env` (Linux) | User | Dipakai untuk | Boleh dikemas ke `.exe`? |
|---|---|---|---|
| `SUPABASE_ADMIN_DB` | `postgres` (pemilik) | Migrasi dan pengaturan dari Linux | **Tidak pernah** |
| `SUPABASE_APP_DB` | `pyrosnap_app` (terbatas) | Aplikasi | Ya, sebagai nilai `PYROSNAP_DB` |

Aplikasi hanya membaca `PYROSNAP_DB`. Agar memakai Supabase, isi `PYROSNAP_DB` di `.env` komputer tersebut dengan nilai `SUPABASE_APP_DB`.

## 2. Yang sudah dipasang

1. Migrasi `001`–`010`. `010_fungsi_akun.sql` menambah `masuk_akun` dan `daftar_publik`: password diverifikasi di database dan akun baru selalu berperan Publik.
2. `database/supabase/akses-aplikasi.sql`: mencabut akses `anon`/`authenticated` (sebelumnya otomatis mendapat 126 hak pada tabel kita) dan memberi hak minimum ke `pyrosnap_app`.
3. Akun uji dari `database/seed-local.sql`: `admin_uji`, `operator_uji`, `publik_uji`.
4. Data API dimatikan saat proyek dibuat. **Jangan diaktifkan**; aplikasi tidak memakainya.

## 3. Hak `pyrosnap_app`

| Tabel | Hak |
|---|---|
| `users` | SELECT `user_id`, `username`, `role` saja (**tanpa `password_hash`**) |
| `hotspot`, `hotspot_verifikasi`, `hotspot_report`, `report_hotspot` | SELECT, INSERT |
| `hotspot_data_source` | SELECT, INSERT, UPDATE `waktu_sinkronisasi` |
| `alert` | SELECT, INSERT, UPDATE `is_read` |
| `filter_criteria` | SELECT, INSERT, DELETE |
| Fungsi `masuk_akun`, `daftar_publik` | EXECUTE |

Terbukti ditolak (42501): membaca hash, membuat Admin, mengubah peran, menghapus akun/titik/peringatan/verifikasi, TRUNCATE, DROP, CREATE TABLE, CREATE ROLE.

## 4. Risiko yang tersisa

Kredensial `pyrosnap_app` ikut di paket, dan pembagian peran (Admin/Operator/Publik) diperiksa di aplikasi. Siapa pun yang mengambil kredensial dari paket masih bisa, tanpa lewat aplikasi:
- membaca titik panas, peringatan, verifikasi, laporan, wilayah pilihan, dan daftar username;
- **menambah** titik panas, peringatan, verifikasi, atau laporan palsu, dan menandai peringatan orang lain sebagai dibaca;
- mencoba menebak password lewat `masuk_akun` (belum ada pembatas percobaan).

Karena itu: **hanya isi data uji, jangan pakai password yang dipakai di layanan lain, bagikan paket hanya ke tester, dan setelah penilaian ganti password atau hapus proyeknya.** Menghilangkan risiko ini butuh server API perantara, di luar lingkup MVP.

## 5. Perawatan

- Paket gratis Supabase dapat menjeda proyek yang lama tidak dipakai. Cek status proyek di dashboard sebelum demo.
- Bila ada tabel baru, tambahkan haknya di `database/supabase/akses-aplikasi.sql` lalu jalankan sebagai `postgres`.
- Bila `SUPABASE_APP_DB` bocor: ganti password role `pyrosnap_app`, perbarui `.env`, lalu kemas ulang.
