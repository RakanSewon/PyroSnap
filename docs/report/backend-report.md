# Laporan Backend PyroSnap

> Kondisi per 2026-11-23, commit `1b2e4df`, branch `545573`. Rincian: `docs/progress/backend-progress.md`, `docs/backend/`.

## 1. Teknologi

| Bagian | Pilihan |
|---|---|
| Platform | .NET 8 LTS, WPF (`net8.0-windows`) |
| Akses database | Npgsql 8.0.5, query berparameter, tanpa ORM |
| Password | Hash BCrypt, diverifikasi di database lewat fungsi `masuk_akun` |
| REST API pihak ketiga | RestSharp 112.1.0 |
| Konfigurasi | Environment variable lalu berkas `.env` (`Services/AppConfig.cs`) |

## 2. Struktur

Class domain mengikuti diagram: `User`, `Hotspot`, `Location`, `FilterCriteria`, `HotspotDataSource`, `HotspotReport`, `Alert`, dan `MapViewer`. `MapViewer` masih kode konsol lama yang tidak dipakai; peta sebenarnya ada di `Views/DashboardView.Peta.cs` dan `Assets/map.html`.

| Kelompok | Berkas di `PyroSnap/Services/` | Fungsi |
|---|---|---|
| Sumber data | `HotspotDataSource` | Mengambil dan mengurai CSV NASA FIRMS |
| Repository | `UserRepository`, `HotspotRepository`, `AlertRepository`, `ReportRepository`, `VerifikasiRepository`, `PreferensiRepository` | Baca dan tulis PostgreSQL |
| Aturan | `AmbangKritis`, `WilayahPantau` | Ambang peringatan dan 8 area pantau |
| Web API | `LayananWeb`, `KualitasUdaraApi`, `CuacaApi`, `WilayahApi` | Layanan pihak ketiga |

## 3. Fitur backend

- **Sinkron NASA FIRMS:** sumber bawaan `VIIRS_NOAA20_NRT`. Kolom dibaca dari header, baris rusak dilewati dan dihitung, waktu disimpan dalam UTC.
- **Tingkat keyakinan:** VIIRS `l/n/h` dipetakan ke 20/60/90, lalu dikelaskan menurut LAPAN: Rendah < 30, Nominal 30–79, Tinggi ≥ 80.
- **Peringatan:** dibuat untuk titik dengan keyakinan ≥ 80, dikirim ke Admin dan Operator, tidak digandakan, maksimal 50 per sinkron.
- **Akun dan peran:** Admin, Operator, Publik (daftar mandiri), dan Tamu (tanpa akun).
- **Laporan:** ekspor CSV dan TXT dengan format angka tetap, tercatat di database dalam satu transaksi.
- **Verifikasi lapangan:** status dan catatan petugas tersimpan beserta riwayatnya.

## 4. Layanan Web API (nilai tambah Modul 10)

| Layanan | Penyedia | Data di aplikasi |
|---|---|---|
| Kualitas udara | Open-Meteo Air Quality | PM2.5, PM10, kategori US AQI, saran singkat |
| Cuaca | Open-Meteo Forecast | Angin, perkiraan arah asap, kelembapan, suhu, hujan |
| Nama wilayah | Nominatim (OpenStreetMap) | Desa, kecamatan, kabupaten/kota, provinsi |

Ketiganya tanpa API key, memakai cache dan batas waktu 10 detik. Nominatim diberi jeda minimal 1,1 detik per permintaan. Bila layanan gagal, data titik panas tetap tampil dengan keterangan "Tidak tersedia". Atribusi sumber ditampilkan di aplikasi.

## 5. Hasil uji backend

| Uji | Lingkungan | Hasil |
|---|---|---|
| Parser data FIRMS asli | Linux | 30/30 |
| Repository dan transaksi | PostgreSQL lokal | 26/26 |
| Peringatan | PostgreSQL lokal | 23/23 |
| Peran, verifikasi, wilayah | PostgreSQL lokal | 37/37 |
| Web API, termasuk panggilan asli | Linux dan internet | 25/25 |
| Repository asli ke Supabase | Linux ke Supabase | 41/41 |

## 6. Batasan

- Paket untuk tester tidak memuat key FIRMS, sehingga tombol "Perbarui dari NASA FIRMS" tidak bisa mengambil data baru dari paket. Data diperbarui dari komputer tim.
- Pembagian peran diperiksa di aplikasi, belum di server API tersendiri.
- Belum ada pembatas percobaan login.
- Kategori udara memakai US AQI; kategori ISPU belum diterapkan.
- Key FIRMS lama masih ada di riwayat git, jadi key tersebut harus sudah dinonaktifkan.
