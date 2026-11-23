# Laporan Database PyroSnap

> Kondisi per 2026-11-23. Dokumen ini tidak memuat password, connection string, atau alamat proyek. Rincian: `docs/database/`, `docs/architecture/database-architecture.md`.

## 1. Ringkasan

| Hal | Nilai |
|---|---|
| DBMS | PostgreSQL |
| Database online | Supabase, PostgreSQL 17, region Singapura |
| Koneksi aplikasi | Session pooler, SSL wajib |
| Jumlah tabel | 9 |
| Migrasi | `000`–`010` di folder `database/` |

## 2. Tabel

| Tabel | Isi |
|---|---|
| `users` | Akun, hash password, peran |
| `hotspot_data_source` | Sumber data dan waktu sinkron terakhir |
| `location` | Data lokasi |
| `filter_criteria` | Kriteria filter dan wilayah pilihan |
| `hotspot` | Titik panas |
| `hotspot_report` | Catatan laporan yang diekspor |
| `report_hotspot` | Relasi laporan dan titik panas |
| `alert` | Peringatan per pengguna |
| `hotspot_verifikasi` | Riwayat verifikasi lapangan |

## 3. Migrasi

| Berkas | Perubahan |
|---|---|
| `000`, `001` | Pembuatan database dan skema awal |
| `002` | Hotspot unik agar sinkron ulang tidak menggandakan data |
| `003` | Waktu memakai `timestamptz` |
| `004` | Format laporan menerima TXT |
| `005`, `006` | Sumber data dan peringatan unik |
| `007` | Data awal sumber FIRMS |
| `008` | Peran Analis digabung ke Admin, peran Publik ditambahkan |
| `009` | Tabel verifikasi lapangan |
| `010` | Fungsi `masuk_akun` dan `daftar_publik` |

## 4. Keamanan

Aplikasi memakai user terbatas `pyrosnap_app`, bukan pemilik database.

| Objek | Hak `pyrosnap_app` |
|---|---|
| `users` | Baca `user_id`, `username`, `role`, tanpa `password_hash` |
| `hotspot`, `hotspot_verifikasi`, `hotspot_report`, `report_hotspot` | SELECT, INSERT |
| `hotspot_data_source` | SELECT, INSERT, UPDATE waktu sinkron |
| `alert` | SELECT, INSERT, UPDATE `is_read` |
| `filter_criteria` | SELECT, INSERT, DELETE |
| `masuk_akun`, `daftar_publik` | EXECUTE |

Akses `anon` dan `authenticated` dari Supabase dicabut, dan Data API dimatikan.

## 5. Isi data saat diuji

| Data | Jumlah |
|---|---|
| Titik panas (NASA FIRMS, 22 Nov 2026) | 691: Tinggi 4, Nominal 617, Rendah 70 |
| Peringatan | 8: Admin 4, Operator 4 |
| Akun uji | 3: Admin, Operator, Publik |

## 6. Hasil uji

- Migrasi dan seed dari database kosong: 6/6 lulus.
- Integrasi Supabase sebagai `pyrosnap_app`: 41/41 lulus.
- 14 aksi terlarang ditolak dengan kode 42501, antara lain membaca hash, membuat Admin, mengubah peran, menghapus data, TRUNCATE, dan DROP.
- Peran `anon` ditolak saat membaca `users`.

## 7. Batasan dan perawatan

- Kredensial `pyrosnap_app` ikut di dalam paket. Pemegangnya masih bisa membaca data, menambah data palsu, dan mencoba menebak password, jadi database hanya boleh berisi data uji.
- Paket hanya dibagikan ke tester dan penilai. Setelah penilaian, ganti password `pyrosnap_app` atau hapus proyek Supabase.
- Supabase gratis dapat menjeda proyek yang lama tidak dipakai, jadi statusnya perlu dicek sebelum demo dan penilaian.
