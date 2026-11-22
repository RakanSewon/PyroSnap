# Fitur Tambahan PyroSnap (di luar class diagram)

> Item di sini di luar 8 class diagram. Item **⬜ belum disetujui** tidak dikerjakan sebelum tim memutuskan; item **✅ disetujui** masuk Fase 7 (`docs/progress/backend-progress.md`).
> Alasan: `log.md` mencatat tambahan di luar diagram pernah ditolak sebagai over-engineering, jadi tiap tambahan dicatat dulu di sini.
> Disusun 2026-11-22. Diperbarui 2026-11-22: keputusan peran Publik & nilai tambah Operator (K9).

## 1. Tujuan

Mendata ide fitur di luar 8 class, nilainya, ketergantungannya, dan ukuran kasarnya, lalu menandai status persetujuan.

## 2. Ringkasan

| ID | Fitur | Nilai | Ukuran | Bergantung pada | Status |
|---|---|---|---|---|---|
| ADD-01 | Air quality (sesuai README) | Tinggi (tujuan proyek) | Besar | Sumber data baru, tabel baru | ⬜ belum disetujui |
| ADD-02 | Skor risiko kebakaran | Sedang–tinggi | Sedang | FT-01 benar, aturan domain | ⬜ belum disetujui |
| ADD-03 | Filter/lokasi tersimpan per user | Sedang | Kecil–sedang | F3 (login DB), tabel `filter_criteria` | 🟨 sebagian disetujui: wilayah pilihan untuk Publik (PUB-01) |
| ADD-04 | Statistik ringkas | Sedang | Kecil | FT-01 benar | 🟨 sebagian disetujui: ringkasan risiko per wilayah (PUB-02) |
| ADD-05 | Export PDF / XLSX | Sedang | Sedang | Library baru, FT-05 | ⬜ belum disetujui |
| ADD-06 | Auto-refresh terjadwal | Sedang | Kecil | F1, F3 | ⬜ belum disetujui |
| ADD-07 | Pencarian 38 provinsi / geocoding | Rendah–sedang | Kecil–sedang | Keputusan wilayah (FT-02c) | ⬜ belum disetujui |
| ADD-08 | Log aktivitas & logging berkas | Sedang | Kecil | – | ⬜ belum disetujui |
| ADD-09 | Peran Publik (warga) + gabung Admin/Analis | Tinggi (warga mendapat informasi) | Sedang | F3, migrasi peran | ✅ disetujui 2026-11-22 |
| ADD-10 | Verifikasi lapangan oleh Operator | Tinggi (saring alarm palsu) | Sedang | Tabel baru, Detail | ✅ disetujui 2026-11-22 |
| ADD-11 | Navigasi ke titik hotspot | Sedang | Kecil | Detail | ✅ disetujui 2026-11-22 |
| ADD-12 | Data historis (rentang tanggal) | Sedang | Kecil–sedang | F4 (data di DB) | ✅ disetujui 2026-11-22 |
| ADD-13 | Panduan & kontak darurat untuk warga | Tinggi untuk Publik | Kecil | Konten diverifikasi tim | ✅ disetujui 2026-11-22 |

## 3. Detail

### ADD-01 — Air quality
README menyebut *"Personalized dashboard for air quality and wildfire risk"*, tetapi air quality belum ada sama sekali.
- Pilihan sumber: API publik kualitas udara (perlu dicek lisensi/kuota); jangan dipilih sebelum tim setuju.
- Dampak: tabel baru, model baru (di luar diagram), panel UI baru.
- [ ] **ADD-01a** — Tim memutuskan apakah air quality masuk lingkup proyek ini.
- [ ] **ADD-01b** — Bila ya: pilih sumber data + catat kuota/lisensi.
- [ ] **ADD-01c** — Bila ya: rancang model, tabel, dan tampilan (dokumen terpisah).

### ADD-02 — Skor risiko kebakaran
- Gabungkan confidence, FRP, kerapatan hotspot di sekitar, dan (opsional) kualitas udara menjadi skor 0–100.
- [ ] **ADD-02a** — Tim menetapkan rumus/aturan (perlu pihak yang paham domain).
- [ ] **ADD-02b** — Skor tampil di Detail dan sebagai filter.

### ADD-03 — Filter & lokasi tersimpan per user
Tabel `filter_criteria` sudah ada di skema (dirancang untuk ini) tetapi tidak dipakai.
- [ ] **ADD-03a** — Simpan/muat `FilterCriteria` per `user_id`.
- [ ] **ADD-03b** — Lokasi favorit per user (butuh tabel/kolom baru bila disetujui).

### ADD-04 — Statistik ringkas
- Jumlah hotspot per tingkat, total FRP, per provinsi/hari.
- [ ] **ADD-04a** — Kartu ringkasan di dashboard dihitung dari daftar hasil filter.
- [ ] **ADD-04b** — Tren harian (butuh data tersimpan di DB, F4).

### ADD-05 — Export PDF / XLSX
Skema `hotspot_report.format` sudah mencantumkan PDF dan XLSX.
- [ ] **ADD-05a** — Pilih library (hindari yang berlisensi berbayar tanpa persetujuan).
- [ ] **ADD-05b** — Implementasi + uji buka di aplikasi pembaca standar.

### ADD-06 — Auto-refresh
- [ ] **ADD-06a** — Interval sinkronisasi dapat diatur (mis. 3 jam; data NRT FIRMS diperbarui berkala).
- [ ] **ADD-06b** — Tidak menimpa filter/pilihan pengguna saat refresh.

### ADD-07 — Pencarian wilayah
Sekarang hanya 8 lokasi hardcoded dengan radius kotak ±3°.
- [ ] **ADD-07a** — Daftar 38 provinsi (pusat koordinat) sebagai data tetap.
- [ ] **ADD-07b** — Radius dalam km, bukan derajat.

### ADD-08 — Logging
- [ ] **ADD-08a** — Log berkas (sync, error, login gagal) tanpa menulis data sensitif.

### ADD-09 — Peran Publik (warga) — ✅ disetujui
Tim ingin warga juga mendapat informasi. Admin dan Analis digabung menjadi **Admin** karena hak aksesnya sama; peran baru **Publik** punya akses dasar seperti Operator, tetapi tanpa nilai tambah posko/lapangan (ADD-10…ADD-12, alert operasional, export). Matriks lengkap: `docs/database/acount-testing.md` §5.
- Warga mendapat: peta, daftar, detail hotspot; ringkasan risiko per wilayah (ADD-04); peringatan berbahasa awam untuk wilayah pilihan (ADD-03); panduan & kontak darurat (ADD-13).
- Alert operasional "segera penanggulangan" **tidak** dikirim ke Publik (mengganti bagian penerima pada K5).
- Cara masuk warga (K10): **daftar mandiri** (akun Publik, boleh ekspor) atau **masuk sebagai tamu** (tanpa akun, tanpa ekspor, wilayah pilihan hanya untuk sesi).
- Rincian kerja: ROLE-01…ROLE-05, PUB-01…PUB-03 di `docs/progress/backend-progress.md` (Fase 7).

### ADD-10 — Verifikasi lapangan — ✅ disetujui
Hotspot satelit belum tentu kebakaran. Operator/Admin menandai status hotspot (*api aktif / sudah padam / bukan kebakaran*) beserta catatan; semua peran, termasuk Publik, melihat status terakhirnya. Butuh tabel baru (mis. `hotspot_verifikasi`: hotspot, user, status, catatan, waktu). Rincian: OPS-01.

### ADD-11 — Navigasi ke titik — ✅ disetujui
Di halaman Detail untuk Operator/Admin: salin koordinat dan buka arah ke lokasi di peta daring. Rincian: OPS-02.

### ADD-12 — Data historis — ✅ disetujui
Operator/Admin memilih rentang tanggal (memakai `FilterCriteria.WaktuMulai/WaktuAkhir` dan `HotspotRepository.QueryAsync`, data sudah tersimpan sejak F4). Publik melihat 24 jam terakhir. Rincian: OPS-03.

### ADD-13 — Panduan & kontak darurat — ✅ disetujui
Halaman informasi untuk warga: apa yang dilakukan saat ada asap/kebakaran, dan nomor penting. **Isi dan nomor kontak wajib diambil dari sumber resmi dan diverifikasi tim**; jangan ditulis dari ingatan. Rincian: PUB-03.

## 4. Cara memutuskan
1. Tiap anggota menandai item yang dianggap perlu untuk tujuan mata kuliah/proyek.
2. Item yang disetujui dipindahkan ke `main-feature.md` dengan ID `FT-xx` dan fase kerja.
3. Item yang ditolak dicatat di `log.md` seperti keputusan sebelumnya.

## 5. Rujukan
`README.md`, `log.md`, `database/001_schema.sql`, `docs/feature/main-feature.md`
