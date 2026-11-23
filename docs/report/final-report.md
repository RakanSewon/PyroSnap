# Laporan Akhir PyroSnap

**Junior Project (Teknik Basis Data, Pemrograman Berbasis Objek)**
**Modul 10: Minimum Viable Product**

| | |
|---|---|
| Nama aplikasi | PyroSnap, pemantauan titik panas kebakaran hutan dan lahan di Indonesia |
| Kelompok | *(isi)* |
| Anggota | *(isi nama dan NIM)* |
| Repositori | https://github.com/RakanSewon/PyroSnap, branch `545573` |
| Tanggal laporan | 23 November 2026 |

## 1. Ringkasan

PyroSnap adalah aplikasi desktop Windows yang menampilkan titik panas dari NASA FIRMS pada peta, mengelompokkan tingkat keyakinannya, memberi peringatan kepada petugas, dan menyediakan informasi bagi warga. Aplikasi dibangun dengan WPF .NET 8 dan PostgreSQL yang berjalan online di Supabase, sehingga tester tidak perlu memasang database.

## 2. Pemenuhan tugas Modul 10

| Tugas | Hasil | Status |
|---|---|---|
| Finalisasi aplikasi | Paket Release berjalan di Windows, 36 bug tercatat | Selesai, menunggu hasil Black Box |
| Layanan web API | Kualitas udara, cuaca, dan nama wilayah | Selesai |
| Panduan penggunaan | `BACA-DULU.txt` dan halaman Panduan di aplikasi | **Panduan pemakaian lengkap belum dibuat** |
| Pengemasan | ZIP self-contained, cukup diekstrak lalu jalankan `.exe` | Selesai |
| Konfigurasi basis data | Database online, dokumentasi di `docs/database/` | **Panduan singkat belum dibuat** |
| Testing format Modul 10 | Worksheet 64 kasus di `docs/debug/blackbox-testing.md` | **Hasil tester belum dimasukkan** |
| Pengumpulan | Daftar isi di bagian 8 | Belum |

## 3. Arsitektur

```text
NASA FIRMS ──> Services ──> Supabase (PostgreSQL)
                  ^                 |
Open-Meteo ───────┤                 v
Nominatim ────────┘          Views WPF + peta Leaflet
```

Rincian tiap bagian ada di `backend-report.md`, `database-report.md`, dan `frontend-report.md`.

## 4. Fitur per peran

| Peran | Kemampuan utama |
|---|---|
| Admin | Seluruh fitur, termasuk sinkron NASA FIRMS |
| Operator | Peringatan, verifikasi lapangan, navigasi ke titik, data historis, ekspor |
| Publik | Peta, wilayah pilihan, kualitas udara, ekspor |
| Tamu | Peta dan informasi tanpa akun, tanpa ekspor |

## 5. Layanan web API

| Layanan | Penyedia | Nilai tambah |
|---|---|---|
| Kualitas udara | Open-Meteo | Warga dan petugas melihat dampak asap |
| Cuaca dan angin | Open-Meteo | Perkiraan arah sebaran asap |
| Nama wilayah | Nominatim/OpenStreetMap | Koordinat menjadi nama wilayah |

## 6. Pengujian

### 6.1 Uji teknis

| Uji | Hasil |
|---|---|
| Parser data FIRMS | 30/30 |
| Repository dan transaksi | 26/26 |
| Peringatan | 23/23 |
| Peran, verifikasi, wilayah | 37/37 |
| Web API | 25/25 |
| Integrasi Supabase, termasuk 14 aksi terlarang | 41/41 |
| Tes awal paket `.exe` di Windows | 5/5 |

### 6.2 Black Box Testing (format Lab 10.2)

| | |
|---|---|
| Tester | *(isi nama teman)* |
| Tanggal pengujian | *(isi)* |
| Komputer | *(isi versi Windows, dan apakah tanpa .NET SDK)* |

| Bagian | Kasus | Sesuai | Tidak Sesuai | Catatan |
|---|---:|---:|---:|---|
| A. Umum | 14 | *(isi)* | *(isi)* | |
| B. Admin | 17 | *(isi)* | *(isi)* | |
| C. Operator | 7 | *(isi)* | *(isi)* | |
| D. Publik | 4 | *(isi)* | *(isi)* | |
| E. Tamu | 3 | *(isi)* | *(isi)* | |
| F. Web API | 7 | *(isi)* | *(isi)* | |
| G. Instalasi | 4 | *(isi)* | *(isi)* | |
| H. Tampilan dan Tema | 8 | *(isi)* | *(isi)* | |
| **Total** | **64** | *(isi)* | *(isi)* | |

### 6.3 Bug

Tercatat 36 bug di `docs/debug/bug-fix.md`: 8 prioritas P0, 10 P1, dan 18 P2. BUG-17 baru selesai sebagian karena `MapViewer.cs` masih berupa kode lama yang tidak dipakai.

## 7. Batasan

- Hanya berjalan di Windows 64-bit dan membutuhkan internet.
- Paket tidak memuat key NASA FIRMS, sehingga data diperbarui dari komputer tim.
- Kredensial database terbatas ada di dalam paket, jadi paket hanya untuk tester dan penilai.
- Wilayah pada filter masih memakai 8 area pantau.
- Kategori udara memakai US AQI, belum ISPU.
- Kalender DatePicker tetap terang pada tema gelap.

## 8. Isi pengumpulan

- [ ] `PyroSnap-Uji-Windows.zip` (`.exe` siap jalan)
- [ ] Panduan pemakaian
- [ ] Panduan singkat konfigurasi basis data
- [ ] Worksheet Black Box yang sudah diisi
- [ ] Laporan akhir ini
- [ ] Tautan repositori

## 9. Kesimpulan

*(Lengkapi setelah hasil Black Box dimasukkan.)*
