# Usulan Perubahan Skema Database

> Status: paket inti **disetujui** 2026-11-22 dan dibuat sebagai migrasi `002`–`007` (diuji pada tabel sementara, belum dijalankan pada DB lokal). `001_schema.sql` tetap tidak diubah. Disusun 2026-11-22.
> Skrip asli ada di folder `database/`; potongan SQL di bawah hanya ringkasan. Buat cadangan sebelum menjalankan migrasi pada database yang berisi data.

## 1. Tujuan

Menyesuaikan skema dengan kebutuhan nyata aplikasi (sync berulang, data UTC, ekspor TXT, confidence VIIRS) tanpa merusak rancangan ERD Modul 4.

## 2. Ringkasan

| ID | Perubahan | Alasan | Terkait | Status |
|---|---|---|---|---|
| SCH-01 | `UNIQUE` pada hotspot (source, lat, lon, waktu) | Cegah duplikat tiap sync; syarat upsert | DBA-04, TST-23 | ✅ disetujui, skrip dibuat & diuji |
| SCH-02 | `TIMESTAMP` → `TIMESTAMPTZ` | FIRMS memakai UTC; hindari salah zona | BUG-04 | ✅ disetujui, skrip dibuat & diuji |
| SCH-03 | CHECK `format` menerima `TXT` | Aplikasi mengekspor TXT | BUG-11, TST-24 | ✅ disetujui, skrip dibuat & diuji |
| SCH-04 | Penanganan confidence VIIRS (`l/n/h`) | Skema `SMALLINT 0–100`, sumber kategorikal | BUG-03 | 🟨 opsi A sementara (di parser) |
| SCH-05 | Indeks `(latitude, longitude)` | Query area/pencarian wilayah | FT-02 | ⏸ ditunda (keputusan 2026-11-22) |
| SCH-06 | `UNIQUE` pada `hotspot_data_source.nama_sumber` | Seed idempoten | SET-03 | ✅ disetujui, skrip dibuat & diuji |
| SCH-07 | Skrip migrasi bernomor + rename file janggal | Migrasi terurut, nama file aman | DBA-03, DBA-10 | ✅ disetujui, skrip dibuat & diuji |
| SCH-08 | `UNIQUE (hotspot_id, target_user_id)` pada `alert` | Cegah alert ganda tiap sync | ALR-05 | ✅ disetujui, skrip dibuat & diuji |
| SCH-09 | Peran: Analis → Admin; CHECK `Admin/Operator/Publik` (`008_peran_publik.sql`) | Keputusan K9/K10 | ROLE-01 | ✅ dibuat & diuji (tabel sementara); jalankan di DB lokal |
| SCH-10 | Tabel `hotspot_verifikasi` + hak user aplikasi (`009_hotspot_verifikasi.sql`) | Verifikasi lapangan (ADD-10) | OPS-01 | ✅ dibuat & diuji (tabel sementara); jalankan di DB lokal |

## 3. Detail

### SCH-01 — Kunci unik hotspot
```sql
ALTER TABLE hotspot
  ADD CONSTRAINT uq_hotspot_source_pos_time
  UNIQUE (source_id, latitude, longitude, waktu_akuisisi);
```
- **Risiko:** bila sudah ada duplikat, perintah gagal; bersihkan dulu. Dua satelit yang mendeteksi titik sama pada waktu sama dari sumber sama dianggap satu.
- **Verifikasi:** TST-23.
- [x] Disetujui  [x] Skrip dibuat (`002_hotspot_unique.sql`)  [x] Diuji (tabel sementara)

### SCH-02 — Waktu dengan zona
```sql
ALTER TABLE hotspot            ALTER COLUMN waktu_akuisisi     TYPE TIMESTAMPTZ USING waktu_akuisisi     AT TIME ZONE 'UTC';
ALTER TABLE hotspot_data_source ALTER COLUMN waktu_sinkronisasi TYPE TIMESTAMPTZ USING waktu_sinkronisasi AT TIME ZONE 'UTC';
ALTER TABLE hotspot_report     ALTER COLUMN tanggal_dibuat     TYPE TIMESTAMPTZ USING tanggal_dibuat     AT TIME ZONE 'UTC';
ALTER TABLE alert              ALTER COLUMN "timestamp"        TYPE TIMESTAMPTZ USING "timestamp"        AT TIME ZONE 'UTC';
-- filter_criteria.waktu_mulai / waktu_akhir bila dipakai (ADD-03)
```
- **Catatan:** `DateTime` .NET dengan `Kind=Utc` yang dikirim ke `timestamptz` diterima Npgsql 8 dengan benar; `Kind=Unspecified/Local` ditolak — kode harus memastikan UTC.
- [x] Disetujui  [x] Skrip dibuat  [x] Diuji (tabel sementara)

### SCH-03 — Format laporan
```sql
ALTER TABLE hotspot_report DROP CONSTRAINT hotspot_report_format_check;
ALTER TABLE hotspot_report ADD CONSTRAINT hotspot_report_format_check
  CHECK (format IN ('TXT','CSV','PDF','XLSX'));
```
- **Catatan:** nama constraint bawaan PostgreSQL diasumsikan `hotspot_report_format_check`; cek dengan `\d hotspot_report` sebelum menjalankan.
- [x] Disetujui  [x] Skrip dibuat  [x] Diuji (tabel sementara)

### SCH-04 — Confidence VIIRS
Sumber VIIRS memberi `l`/`n`/`h`; diagram dan skema memakai angka 0–100.
- **Opsi A (rekomendasi):** petakan di parser: `l=20` (diperbarui dari 30 pada 2026-11-22 agar masuk kelas Rendah LAPAN 0–29), `n=60`, `h=90`; **skema tidak berubah**, tetap sesuai diagram. Konsekuensi: angka bukan pengukuran asli; dokumentasikan di UI ("Rendah/Nominal/Tinggi").
- **Opsi B:** tambah kolom `confidence_label VARCHAR(10)` dan simpan keduanya.
- **Opsi C:** ubah `confidence` jadi teks (menyimpang dari diagram; tidak disarankan).
- Catatan: UI memakai kelas LAPAN (Rendah < 30, Nominal 30–79, Tinggi ≥ 80; `Views/TingkatHotspot.cs`), cocok dengan Opsi A (l=20, n=60, h=90).
- [x] Opsi dipilih: **A (sementara, dipakai di F1)**  [x] Diterapkan di parser  [x] Diuji pada data asli
- *2026-11-22:* opsi A dipakai sebagai default supaya F1 bisa jalan; skema tidak berubah. Tim tetap bisa memilih B/C nanti (K3).

### SCH-05 — Indeks koordinat (opsional)
```sql
CREATE INDEX idx_hotspot_lat_lon ON hotspot (latitude, longitude);
```
- Ditunda sampai query area terbukti lambat.
- [ ] Disetujui (ditunda)

### SCH-06 — Nama sumber unik
```sql
ALTER TABLE hotspot_data_source ADD CONSTRAINT uq_source_nama UNIQUE (nama_sumber);
```
- [x] Disetujui  [x] Skrip dibuat (`005_source_unique.sql`)

### SCH-07 — Migrasi bernomor
Struktur (dibuat 2026-11-22):
```
database/
├── 001_schema.sql            (dulu pyrosnap_postgres.sql)
├── 002_hotspot_unique.sql    (SCH-01)
├── 003_timestamptz.sql       (SCH-02)
├── 004_report_format_txt.sql (SCH-03)
├── 005_source_unique.sql     (SCH-06)
├── 006_alert_unique.sql      (SCH-08)
├── 007_seed_data_source.sql  (seed sumber)
├── migrate.sh                (runner)
└── 000_create_database.sql   (dulu "CREATE DATABASE pyrosnap;.sql")
```
- **Aturan:** satu perubahan per berkas, tiap berkas dapat dijalankan sekali di atas versi sebelumnya; tanpa alat migrasi.
- [x] Disetujui  [x] Berkas dibuat (`000`–`007`, `migrate.sh`)  [x] Diuji dari DB kosong (tabel sementara)

### SCH-08 — Alert tanpa duplikat
```sql
ALTER TABLE alert
  ADD CONSTRAINT uq_alert_hotspot_user UNIQUE (hotspot_id, target_user_id);
```
- Memungkinkan `INSERT ... ON CONFLICT DO NOTHING` saat sync ulang (ALR-05).
- [x] Disetujui  [x] Skrip dibuat  [x] Diuji (tabel sementara)

## 4. Checklist target
- [x] **SCH-A1** — Tim memutuskan SCH-01…SCH-08 — 2026-11-22: paket inti disetujui, SCH-05 ditunda.
- [x] **SCH-A2** — Opsi SCH-04 dipilih dan dicatat di `backend/data-source-api.md`.
- [ ] **SCH-A3** — Cadangan DB dibuat sebelum menjalankan migrasi pada DB berisi data.
- [x] **SCH-A4** — Setelah semua migrasi: TST-20…TST-26 lulus. (Diuji pada tabel sementara; ulangi setelah migrasi DB asli.)
- [ ] **SCH-A5** — `001_schema.sql` / ERD diperbarui agar konsisten.

## 5. Rujukan
`database/001_schema.sql`, `docs/architecture/database-architecture.md`, `docs/debug/bug-fix.md`, `docs/debug/testing-schema.md`
