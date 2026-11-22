# Setup Database Lokal & Seed

> Status: **prosedur berlaku** (Fase 2, 2026-11-22). Skrip migrasi sudah diuji pada tabel sementara; belum dijalankan pada DB lokal (butuh user `postgres`).
> Tidak ada password, key, atau connection string nyata di dokumen ini; semuanya memakai penanda `<...>`.

## 1. Tujuan

Prosedur satu jalur supaya semua anggota tim bisa membangun database PyroSnap dari nol dengan hasil sama.

## 2. Prasyarat

- PostgreSQL (versi yang dipakai tim; catat di sini setelah disepakati: `____`).
- `psql` tersedia.
- .NET 8 SDK dan Windows + WebView2 Runtime untuk menjalankan aplikasi WPF.

## 3. Langkah

Berkas di `database/` (dijalankan berurutan, masing-masing **sekali**, sebagai pemilik tabel, mis. `postgres`):

| Berkas | Isi |
|---|---|
| `000_create_database.sql` | `CREATE DATABASE pyrosnap;` (dulu bernama `CREATE DATABASE pyrosnap;.sql`) |
| `001_schema.sql` | Skema 8 tabel ERD Modul 4 (dulu `pyrosnap_postgres.sql`) |
| `002_hotspot_unique.sql` | SCH-01 kunci unik hotspot |
| `003_timestamptz.sql` | SCH-02 kolom waktu → `TIMESTAMPTZ` |
| `004_report_format_txt.sql` | SCH-03 format laporan menerima `TXT` |
| `005_source_unique.sql` | SCH-06 nama sumber unik |
| `006_alert_unique.sql` | SCH-08 alert unik per (hotspot, penerima) |
| `007_seed_data_source.sql` | Seed sumber "NASA FIRMS (VIIRS_NOAA20_NRT)", idempoten |
| `010_fungsi_akun.sql` | Login dan daftar lewat fungsi database (`masuk_akun`, `daftar_publik`); **wajib** untuk versi aplikasi terbaru |
| `008_peran_publik.sql` | Analis → Admin; peran Admin/Operator/Publik (K9/K10) |
| `009_hotspot_verifikasi.sql` | Tabel verifikasi lapangan + hak user aplikasi (OPS-01) |
| `migrate.sh` | Menjalankan berkas bernomor mulai nomor tertentu, berhenti di error pertama |

1. **Buat database** (nama `pyrosnap`), sekali saja:
   ```bash
   createdb -h localhost -U postgres pyrosnap
   ```
2. **Jalankan skema + migrasi.**
   - Database **baru/kosong** (belum ada tabel):
     ```bash
     database/migrate.sh 001 -h localhost -U postgres -d pyrosnap
     ```
   - Database yang **sudah sampai `007`** (kondisi DB lokal per 2026-11-22, setelah Fase 2):
     ```bash
     cat database/008_*.sql database/009_*.sql | sudo -u postgres psql -v ON_ERROR_STOP=1 -d pyrosnap
     ```
   - Database yang **baru berisi `001_schema.sql`**:
     ```bash
     database/migrate.sh 002 -h localhost -U postgres -d pyrosnap
     ```
   Tiap berkas memakai `BEGIN/COMMIT`; bila satu gagal, berkas itu dibatalkan seluruhnya dan skrip berhenti.
   User aplikasi (mis. `pyrosnap_app`) tidak bisa menjalankan migrasi karena bukan pemilik tabel.
3. **Hak user aplikasi.** User aplikasi butuh `SELECT, INSERT, UPDATE, DELETE` di semua tabel dan `USAGE` di sequence (sudah ada di DB lokal per 2026-11-22). Untuk DB baru, sebagai `postgres`:
   ```sql
   GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO <user-aplikasi>;
   GRANT USAGE ON ALL SEQUENCES IN SCHEMA public TO <user-aplikasi>;
   ```
4. **Seed sumber data:** sudah termasuk di `007_seed_data_source.sql`. URL berisi penanda `{KEY}`; key asli **tidak pernah** disimpan di tabel, hanya di `.env`.
5. **Seed akun uji:** ikuti `docs/database/acount-testing.md` §4 (skrip lokal `database/seed-local.sql`, sudah di-gitignore).
6. **Atur koneksi aplikasi** (pilih satu):
   - Environment variable `PYROSNAP_DB`
   - atau file `.env` di root repo (di-gitignore; dibaca setelah DBA-01 selesai)
   Format: `Host=<host>;Port=5432;Database=pyrosnap;Username=<user>;Password=<password>`

   Variabel yang dibaca `Services/AppConfig.cs` (environment variable menang atas `.env`; `.env` dicari dari folder aplikasi naik hingga 5 tingkat):

   | Variabel | Wajib | Kegunaan |
   |---|---|---|
   | `PYROSNAP_DB` | Ya, untuk fitur database | Connection string PostgreSQL |
   | `FIRMS_MAP_KEY` | Ya, untuk data live | Key NASA FIRMS; tanpa ini dashboard memakai data contoh |
   | `CARTO_KEY` | Tidak | Key tile CARTO bila penyedia mewajibkannya |
   | `FIRMS_SOURCE` | Tidak | Produk FIRMS; default `VIIRS_NOAA20_NRT` (alternatif `VIIRS_NOAA21_NRT`, `MODIS_NRT`) |

   Contoh isi `.env` (nilai diganti milik sendiri, jangan di-commit):
   ```
   PYROSNAP_DB=Host=localhost;Port=5432;Database=pyrosnap;Username=<user>;Password=<password>
   FIRMS_MAP_KEY=<key-firms>
   CARTO_KEY=<key-carto-opsional>
   ```
7. **Cek koneksi:** halaman Login menampilkan baris "Database: Terhubung ke database 'pyrosnap' …" atau "Database tidak tersedia: <alasan>" (merah). Pengecekan berjalan di latar belakang; aplikasi tetap bisa dipakai tanpa database.

## 4. Verifikasi cepat

```sql
\dt                                  -- 8 tabel
SELECT count(*) FROM hotspot_data_source;   -- 1 setelah seed
SELECT username, role FROM users;           -- akun uji (tanpa kolom hash)
```

## 5. Reset ulang (hanya DB lokal pengembangan)

```bash
dropdb -U <user-postgres> pyrosnap && createdb -U <user-postgres> pyrosnap
```
Jangan dijalankan pada database bersama tanpa persetujuan dan cadangan.

## 6. Checklist target

- [ ] **SET-01** — Versi PostgreSQL yang dipakai tim disepakati dan dicatat. (DB lokal ketua: PostgreSQL 18.6.)
- [ ] **SET-02** — Prosedur §3 dicoba oleh minimal 2 anggota pada mesin berbeda; hasil `\dt` sama.
- [x] **SET-03** — Seed `hotspot_data_source` idempoten (`005` + `ON CONFLICT DO NOTHING`; diuji 2× jalan = 1 baris).
- [x] **SET-04** — `.gitignore` memuat `.env` dan `database/seed-local.sql`.
- [x] **SET-05** — README memuat tautan ke dokumen ini dan ringkasan langkah (tanpa rahasia).
- [x] **SET-06** — Berkas `CREATE DATABASE pyrosnap;.sql` diganti nama menjadi `000_create_database.sql`.
- [x] **SET-07** — Migrasi `002`–`007` dijalankan pada DB lokal sebagai `postgres` (langkah 2, varian "sudah berisi 001").

## 7. Keputusan terbuka
1. Database dipakai bersama di satu server tim, atau tiap anggota punya DB lokal? (Disarankan lokal untuk pengembangan.)
2. Pengguna PostgreSQL khusus aplikasi dengan hak terbatas (bukan `postgres`) — disarankan.

## 8. Rujukan
`database/001_schema.sql`, `docs/database/schema-changes.md`, `docs/database/acount-testing.md`, `docs/architecture/database-architecture.md`
