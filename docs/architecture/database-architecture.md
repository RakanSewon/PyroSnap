# Arsitektur Database PyroSnap

> Status dokumen: **rencana**. Skema di `database/001_schema.sql` belum diubah dan belum dipakai kode. Disusun 2026-11-22.

## 1. Tujuan

Menghubungkan aplikasi ke PostgreSQL **bertahap** sampai target akhir:
login via tabel `users` (hash), hotspot hasil sync tersimpan, laporan tercatat, alert tersimpan. Setiap tahap kecil dan bisa diuji sendiri.

## 2. Kondisi saat ini

- Skema 8 tabel sudah ada (`database/001_schema.sql`), dirancang dari ERD Modul 4.
- Paket `Npgsql 8.0.5` sudah ditambahkan; `Services/DatabaseConnection.cs` punya `OpenAsync()` dan `TestConnectionAsync()` tetapi **tidak dipanggil di mana pun**.
- Connection string dibaca dari env `PYROSNAP_DB`; bila kosong memakai `Host=localhost;Port=5432;Database=pyrosnap;Username=postgres` (tanpa password). File `.env` ada (di-gitignore) tetapi **tidak dibaca aplikasi**.
- Tidak ada seed data, tidak ada skrip migrasi, tidak ada kunci unik pada `hotspot`.
- *(Selesai 2026-11-22)* File `CREATE DATABASE pyrosnap;.sql` diganti `000_create_database.sql`; migrasi bernomor `002`–`007` + `migrate.sh` ditambahkan.

## 3. Pemetaan tabel ↔ class

| Tabel | Class diagram | Dipakai pada fase | Catatan |
|---|---|---|---|
| `users` | User / Account | F3 | `password_hash` wajib hash; `role` ∈ Admin/Analis/Operator |
| `hotspot_data_source` | HotspotDataSource | F2 (seed), F4 | Baris "NASA FIRMS"; `waktu_sinkronisasi` diperbarui saat sync |
| `hotspot` | Hotspot | F4 | Butuh kunci unik untuk upsert (lihat `database/schema-changes.md`) |
| `location` | Location | F4 (opsional) | Titik referensi wilayah / pusat peta |
| `filter_criteria` | FilterCriteria | Tambahan (lihat `feature/additional-feature.md`) | Filter tersimpan per user; di luar fase wajib |
| `hotspot_report` | HotspotReport | F4 | `format` perlu menerima `TXT` (sekarang hanya PDF/CSV/XLSX) |
| `report_hotspot` | (relasi agregasi) | F4 | Junction laporan ↔ hotspot |
| `alert` | Notification / Alert | F5 | FK ke `hotspot` dan `users` |

### Relasi penting (dari skema)
- `hotspot.source_id → hotspot_data_source` (RESTRICT), `hotspot.location_id → location` (SET NULL)
- `hotspot_report.user_id → users` (RESTRICT)
- `alert.hotspot_id → hotspot`, `alert.target_user_id → users` (CASCADE)
- `report_hotspot` PK gabungan (`report_id`, `hotspot_id`), CASCADE dua arah

## 4. Strategi akses data

1. **Npgsql langsung**, tanpa ORM, pada fase awal. Alasan: 8 tabel sederhana, menghindari dependensi baru (selaras log.md).
2. **Semua query berparameter** (`NpgsqlParameter`/`AddWithValue`); dilarang menggabung string SQL dengan input.
3. **Satu koneksi per operasi** (`await using var conn = await DatabaseConnection.OpenAsync()`); pooling bawaan Npgsql.
4. **Upsert** hotspot: `INSERT ... ON CONFLICT (source_id, latitude, longitude, waktu_akuisisi) DO NOTHING` (butuh SCH-01).
5. **Batch insert dalam satu transaksi** untuk hasil sync (ribuan baris per hari).
6. **Waktu disimpan UTC** (target `TIMESTAMPTZ`, SCH-02); konversi ke waktu lokal hanya di UI.
7. **Koneksi gagal bukan alasan crash**: aplikasi memberi pesan jelas dan tetap bisa memakai data dari API (fase transisi).

## 5. Konfigurasi koneksi

```
PYROSNAP_DB  = Host=...;Port=5432;Database=pyrosnap;Username=...;Password=...
```
- Sumber nilai: environment variable → file `.env` lokal (di-gitignore) → *tanpa default berpassword*.
- Tidak ada connection string/password di kode atau di dokumen.
- Dokumentasi pengaturan ada di `docs/database/setup-and-seed.md`.

## 6. Checklist target

- [x] **DBA-01** — `AppConfig` membaca `PYROSNAP_DB` dari env lalu `.env`. *Selesai bila* aplikasi terhubung memakai nilai `.env` tanpa set env manual.
- [x] **DBA-02** — `TestConnectionAsync()` dipanggil saat start (log/indikator status), tidak memblokir UI dan tidak membuat crash saat DB mati. *Selesai bila* DB dimatikan → aplikasi tetap terbuka dengan pesan jelas.
- [x] **DBA-03** — Terapkan perubahan skema yang disetujui (`SCH-xx`) lewat skrip bernomor di folder `database/`. *Selesai bila* skema bersih dari nol dan hasil upgrade identik.
- [x] **DBA-04** — `HotspotRepository.UpsertAsync(IEnumerable<Hotspot>)` dengan transaksi + `ON CONFLICT`. *Selesai bila* sync dua kali berturut-turut tidak menambah baris.
- [x] **DBA-05** — `HotspotRepository.QueryAsync(FilterCriteria)` menghasilkan hasil sama dengan `FilterCriteria.IsMatch` di memori. *Selesai bila* uji perbandingan lolos pada data uji.
- [x] **DBA-06** — `UserRepository.FindByUsernameAsync` (untuk login). *Selesai bila* hanya mengembalikan `user_id`, `username`, `password_hash`, `role`.
- [x] **DBA-07** — `ReportRepository.SaveAsync(report, userId)` mengisi `hotspot_report` + `report_hotspot` dalam satu transaksi. *Selesai bila* gagal sebagian → tidak ada data setengah jadi.
- [x] **DBA-08** — `AlertRepository` (insert, daftar belum dibaca, `MarkAsRead`). *Selesai bila* sesuai `Alert.IsRead`.
- [x] **DBA-09** — Semua query diaudit: tidak ada penggabungan string SQL. *Selesai bila* `grep` menemukan nol `$"... SELECT/INSERT ... {` dengan input pengguna.
- [x] **DBA-10** — Rename `CREATE DATABASE pyrosnap;.sql` dan dokumentasikan urutan eksekusi skrip. *Selesai bila* README/`setup-and-seed.md` menjelaskan urutan dari nol.

## 7. Keputusan terbuka

1. Pemetaan confidence VIIRS (`l/n/h`) → angka atau ubah tipe kolom (lihat `schema-changes.md` SCH-04).
2. Apakah `location` diisi otomatis dari hotspot (reverse lookup) atau hanya dari daftar wilayah tetap.
3. Retensi data: hotspot disimpan berapa lama (tanpa batas / 30 hari)?
4. Apakah migrasi cukup skrip SQL bernomor atau perlu alat migrasi (disarankan skrip saja).

## 8. Rujukan

- `database/001_schema.sql`, `docs/database/schema-changes.md`, `docs/database/setup-and-seed.md`, `docs/architecture/backend-architecture.md`
