# Backend: Layanan Web API Pihak Ketiga (Modul 10)

> Status: **rencana** (keputusan 2026-11-23). Belum ada kode. Bagian dari `docs/MVP.md` butir 2.
> Ketentuan tiap layanan di bawah dikutip dari halaman resmi penyedia (lihat Rujukan); nama parameter yang **belum terkonfirmasi** ditandai dan wajib dicek di dokumentasi sebelum implementasi.

## 1. Arti "layanan web API" di Modul 10

Tugas kelompok Modul 10 meminta *"Tambahkan layanan web api untuk memperoleh nilai tambah."* Konteks modulnya adalah bab **"Mengakses Layanan Pihak Ketiga"** dan **Lab 10.1 "Terhubung dengan REST Services"**: aplikasi desktop memanggil REST API pihak ketiga (contoh lab: RajaOngkir, dengan kelas statis `Ongkir.cs` dan pustaka RestSharp). Jadi yang dikerjakan adalah **mengonsumsi REST API pihak ketiga**, bukan membangun server API sendiri (usulan server ASP.NET Core di versi awal `MVP.md` dibatalkan).

PyroSnap sudah memakai satu REST API (NASA FIRMS, CSV, lewat `HttpClient`). Nilai tambah datang dari layanan **baru** di bawah ini.

## 2. Keputusan (2026-11-23)

| # | Keputusan |
|---|---|
| W1 | Tambah tiga layanan: **kualitas udara**, **cuaca & arah angin**, **nama wilayah dari koordinat** |
| W2 | Pustaka **RestSharp** seperti Lab 10.1 (NASA FIRMS tetap memakai `HttpClient` yang sudah teruji) |

## 3. Layanan

### 3.1 Kualitas udara — Open-Meteo Air Quality API
- **Endpoint:** `https://air-quality-api.open-meteo.com/v1/air-quality` dengan `latitude`, `longitude`.
- **Data:** PM2.5, PM10, indeks kualitas udara (`european_aqi`; tersedia juga `us_aqi`), bersumber dari Copernicus Atmosphere Monitoring Service (CAMS). Skala `european_aqi`: 0–20 baik … 80–100 sangat buruk, > 100 ekstrem buruk.
- **Key:** tidak perlu.
- **Nilai tambah:** sesuai tujuan README (*"air quality and wildfire risk"*, ADD-01); warga melihat kualitas udara wilayahnya, petugas melihat dampak asap di sekitar titik.
- ⚠ Cek di dokumentasi: nama variabel untuk blok `current` vs `hourly`.

### 3.2 Cuaca & arah angin — Open-Meteo Forecast API
- **Endpoint:** `https://api.open-meteo.com/v1/forecast` dengan `latitude`, `longitude`.
- **Data:** kecepatan angin 10 m (`wind_speed_10m`), kelembapan relatif (`relative_humidity_2m`), arah angin 10 m (`wind_direction_10m`).
- **Key:** tidak perlu.
- **Nilai tambah:** perkiraan arah sebaran asap dan kondisi kering di sekitar titik (petugas); informasi kondisi di wilayah warga.
- ⚠ Cek di dokumentasi: `wind_direction_10m` dan `relative_humidity_2m` sebagai variabel `current` belum terkonfirmasi (yang terkonfirmasi: `wind_speed_10m` di `current`, kelembapan di `hourly`).

**Ketentuan Open-Meteo (berlaku untuk 3.1 dan 3.2):** data berlisensi **CC BY 4.0**; setiap tampilan data wajib mencantumkan atribusi *"Weather data by Open-Meteo.com"* dengan tautan. Tingkat gratis **hanya untuk non-komersial** dengan batas **< 10.000 panggilan/hari, 5.000/jam, 600/menit**. Tugas kuliah termasuk non-komersial.

### 3.3 Nama wilayah dari koordinat — Nominatim (OpenStreetMap), reverse geocoding
- **Endpoint:** `https://nominatim.openstreetmap.org/reverse` dengan `lat`, `lon`, `format=jsonv2`.
- **Data:** nama desa/kecamatan/kabupaten/provinsi untuk sebuah titik.
- **Key:** tidak perlu, tetapi **kebijakan pemakaian ketat**:
  - maksimal **1 permintaan/detik untuk seluruh aplikasi** (jumlah semua pengguna);
  - wajib **User-Agent yang mengidentifikasi aplikasi** — bukan User-Agent bawaan pustaka, dan **dilarang** memalsukan User-Agent browser;
  - permintaan periodik/massal (bulk) sangat tidak dianjurkan; **wajib cache**;
  - atribusi OpenStreetMap (data ODbL).
- **Nilai tambah:** nama wilayah administratif nyata untuk titik, menggantikan perkiraan 8 area pantau (keputusan sementara K2).
- **Konsekuensi desain:** **tidak** dipakai untuk semua titik setiap sinkronisasi (bisa ratusan titik). Hanya **berdasarkan permintaan** (saat Detail dibuka), dengan jeda ≥ 1 detik antar-permintaan dan cache.

## 4. Penempatan di aplikasi

| Tempat | Isi | Peran | Layanan |
|---|---|---|---|
| Detail titik panas | Nama wilayah administratif | Semua | 3.3 |
| Detail titik panas | Kualitas udara (PM2.5, indeks) di titik | Semua | 3.1 |
| Detail titik panas | Angin (kecepatan + arah) dan kelembapan | Semua (arah angin terutama untuk petugas) | 3.2 |
| Banner warga / Wilayah Saya | Kualitas udara di pusat wilayah pilihan | Publik, Tamu | 3.1 |
| Detail & banner | Atribusi "Weather data by Open-Meteo.com" / "© OpenStreetMap contributors" | Semua | 3.1–3.3 |

## 5. Rancangan teknis (mengikuti pola Lab 10.1)

- **Kelas layanan statis** di `Services/` (pola `Ongkir.cs` di lab): `KualitasUdaraApi`, `CuacaApi`, `WilayahApi`, masing-masing memakai `RestClient` dengan `RestRequest` dan mengembalikan model kecil (`KualitasUdara`, `KondisiCuaca`, `NamaWilayah`).
- **Timeout** pendek (±10 detik); gagal = nilai "tidak tersedia", **tidak** menghentikan Detail/Dashboard (FLOW-05).
- **Cache di memori** per koordinat dibulatkan (mis. 2 desimal): kualitas udara & cuaca berlaku ±1 jam; nama wilayah berlaku sampai aplikasi ditutup. Opsi lanjutan: simpan nama wilayah ke database (tabel `location`/`hotspot.location_id`) — butuh keputusan (menyentuh K2).
- **Pembatas laju** untuk Nominatim: satu antrean, jeda ≥ 1 detik.
- **User-Agent** Nominatim: `PyroSnap/1.0 (+https://github.com/RakanSewon/PyroSnap)`; bisa diganti lewat konfigurasi. Jangan memakai email pribadi anggota.
- **Konfigurasi:** URL dasar sebagai konstanta; tidak ada key. Tidak ada rahasia baru di `.env`.
- **JSON:** `System.Text.Json` (bawaan .NET 8) atau serializer bawaan RestSharp; tidak perlu Newtonsoft.

### Versi RestSharp (perlu diputuskan, WEB-00)
Modul 10 menyarankan **RestSharp 106.11.4 + Newtonsoft.Json 12.0.3** untuk proyek **.NET Framework 4.7.2**. PyroSnap memakai **.NET 8**. Pertimbangan:
- API RestSharp versi 107 ke atas berbeda dari contoh lab (mis. `IRestResponse`/`client.Execute` di 106 vs pola async di versi baru).
- Sepengetahuan saya, RestSharp **sebelum 106.12.0** terkena kerentanan ReDoS (CVE-2021-27293) — **perlu diverifikasi** sebelum memilih 106.11.4.
- Usulan: versi stabil terbaru yang mendukung .NET 8; tulis kodenya mengikuti struktur lab (kelas statis + method per layanan) agar tetap mudah dikenali saat penilaian.

## 6. Checklist target

- [ ] **WEB-00** — Pilih versi RestSharp (lihat §5) dan verifikasi kerentanan versi 106.11.4.
- [ ] **WEB-01** — Cek dokumentasi resmi: nama variabel `current`/`hourly` Open-Meteo (udara & cuaca) dan format respons `jsonv2` Nominatim; catat di dokumen ini.
- [ ] **WEB-02** — `KualitasUdaraApi` + model + cache + timeout.
- [ ] **WEB-03** — `CuacaApi` + model + cache + timeout (arah angin ditampilkan sebagai arah mata angin, mis. "dari Tenggara").
- [ ] **WEB-04** — `WilayahApi` + User-Agent identitas + pembatas 1 permintaan/detik + cache; hanya saat Detail dibuka.
- [ ] **WEB-05** — Detail titik: bagian "Kondisi sekitar" (wilayah, kualitas udara, angin, kelembapan) dengan status memuat dan "tidak tersedia".
- [ ] **WEB-06** — Banner warga: kualitas udara wilayah pilihan (dengan bahasa awam, mis. kategori baik/sedang/tidak sehat).
- [ ] **WEB-07** — Atribusi Open-Meteo dan OpenStreetMap tampil di setiap tempat data ditampilkan.
- [ ] **WEB-08** — Penanganan gagal: offline, timeout, respons tidak dikenal, dibatasi penyedia (HTTP 429/403) → pesan jelas, aplikasi tetap jalan.
- [ ] **WEB-09** — Kasus uji Black Box bagian F di `docs/debug/blackbox-testing.md` dijalankan di Windows.
- [ ] **WEB-10** — Panduan pemakaian (MVP-16) menjelaskan data tambahan dan sumbernya.
- [ ] **WEB-11** — (Opsional, keputusan) simpan nama wilayah ke database untuk menyelesaikan K2 secara permanen.

## 7. Keputusan terbuka
1. Versi RestSharp (WEB-00).
2. Nama wilayah disimpan ke database atau cukup cache memori (WEB-11, K2).
3. Indeks kualitas udara yang ditampilkan: `european_aqi`, `us_aqi`, atau keduanya; dan cara menerjemahkannya ke kategori bahasa awam.

## 8. Rujukan
- Modul 10 — Minimum Viable Product (`docs/MODUL 10 - FIX.pdf`): bab Mengakses Layanan Pihak Ketiga, Lab 10.1, Tugas Kelompok
- [Open-Meteo Air Quality API — dokumentasi](https://open-meteo.com/en/docs/air-quality-api)
- [Open-Meteo — Licence (CC BY 4.0, atribusi)](https://open-meteo.com/en/licence)
- [Open-Meteo — Terms (batas tingkat gratis, non-komersial)](https://open-meteo.com/en/terms)
- [Open-Meteo — dokumentasi Forecast API](https://open-meteo.com/en/docs)
- [Nominatim Usage Policy (OSMF)](https://operations.osmfoundation.org/policies/nominatim/)
- [Nominatim — OpenStreetMap Wiki](https://wiki.openstreetmap.org/wiki/Nominatim)
