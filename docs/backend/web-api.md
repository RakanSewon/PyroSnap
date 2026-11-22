# Backend: Layanan Web API Pihak Ketiga (Modul 10)

> Status: **sudah dikodekan 2026-11-23** (WEB-00…08, §9). Uji otomatis 25/25 lulus di Linux termasuk panggilan asli; tampilan di Windows belum dilihat (WEB-09). Bagian dari `docs/MVP.md` butir 2.
> Ketentuan tiap layanan dikutip dari halaman resmi penyedia (lihat Rujukan); parameter dan bentuk respons dikonfirmasi dengan memanggil API sungguhan (§3a).

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
- ✅ Terkonfirmasi (§3a): `current=pm2_5,pm10,us_aqi,european_aqi` berjalan; nilai `current` per jam (`interval` 3600).

### 3.2 Cuaca & arah angin — Open-Meteo Forecast API
- **Endpoint:** `https://api.open-meteo.com/v1/forecast` dengan `latitude`, `longitude`.
- **Data:** kecepatan angin 10 m (`wind_speed_10m`), kelembapan relatif (`relative_humidity_2m`), arah angin 10 m (`wind_direction_10m`).
- **Key:** tidak perlu.
- **Nilai tambah:** perkiraan arah sebaran asap dan kondisi kering di sekitar titik (petugas); informasi kondisi di wilayah warga.
- ✅ Terkonfirmasi (§3a): `wind_speed_10m`, `wind_direction_10m`, `relative_humidity_2m`, `temperature_2m`, `precipitation` semuanya bisa di `current` (interval 15 menit). `wind_direction_10m` = arah **asal** angin dalam derajat (meteorologi).

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

## 3a. Hasil uji panggil langsung (2026-11-23)

Dipanggil dengan `curl` dari Linux untuk koordinat Riau (0,5; 101,5). Tanpa key.

**Kualitas udara** — `GET https://air-quality-api.open-meteo.com/v1/air-quality?latitude=0.5&longitude=101.5&current=pm2_5,pm10,us_aqi,european_aqi&timezone=auto` → HTTP 200:
```json
{"latitude":0.5,"longitude":101.5,"timezone":"Asia/Jakarta",
 "current_units":{"time":"iso8601","interval":"seconds","pm2_5":"μg/m³","pm10":"μg/m³","us_aqi":"USAQI","european_aqi":"EAQI"},
 "current":{"time":"2026-11-23T01:00","interval":3600,"pm2_5":7.4,"pm10":7.5,"us_aqi":41,"european_aqi":25}}
```

**Cuaca & angin** — `GET https://api.open-meteo.com/v1/forecast?latitude=0.5&longitude=101.5&current=wind_speed_10m,wind_direction_10m,relative_humidity_2m,temperature_2m,precipitation&wind_speed_unit=kmh&timezone=auto` → HTTP 200:
```json
{"latitude":0.52724075,"longitude":101.48786,"timezone":"Asia/Jakarta",
 "current_units":{"wind_speed_10m":"km/h","wind_direction_10m":"°","relative_humidity_2m":"%","temperature_2m":"°C","precipitation":"mm"},
 "current":{"time":"2026-11-23T01:45","interval":900,"wind_speed_10m":1.8,"wind_direction_10m":204,"relative_humidity_2m":98,"temperature_2m":24.8,"precipitation":0.10}}
```
- Koordinat di respons bisa sedikit bergeser (titik grid model), jadi jangan dipakai sebagai kunci cache; pakai koordinat permintaan yang dibulatkan.
- Parameter salah → **HTTP 400** dengan `{"error":true,"reason":"..."}`. Kode harus membaca `reason` untuk log, bukan menampilkannya mentah ke pengguna.
- Waktu `current` memakai zona `timezone=auto` (WIB/WITA/WIT sesuai titik).

**Nama wilayah** — `GET https://nominatim.openstreetmap.org/reverse?lat=0.5&lon=101.5&format=jsonv2&zoom=10&addressdetails=1`, header `User-Agent: PyroSnap/1.0 (+https://github.com/RakanSewon/PyroSnap)`, `Accept-Language: id` → HTTP 200:
```json
{"licence":"Data © OpenStreetMap contributors, ODbL 1.0. http://osm.org/copyright",
 "lat":"0.5016731","lon":"101.5168645","addresstype":"suburb","name":"Sialang Sakti",
 "display_name":"Sialang Sakti, Pekanbaru, Tenayan Raya, Riau, Indonesia",
 "address":{"suburb":"Sialang Sakti","city":"Pekanbaru","district":"Tenayan Raya","state":"Riau","country":"Indonesia","country_code":"id"}}
```
- `lat`/`lon` berupa **teks**, bukan angka.
- Kunci di `address` **berbeda per tempat** (`village`, `suburb`, `city`, `county`, `district`, `state`, …). Susun nama dari kunci yang ada, urutan: desa/kelurahan → kota/kabupaten → provinsi.
- Titik di laut (−5; 125) tetap dibalas HTTP 200 dengan nama provinsi saja (`"addresstype":"state"`, Sulawesi Tenggara), karena batas wilayah OSM mencakup laut. Tampilkan apa adanya dengan label "perkiraan".
- Bila sama sekali tidak ada hasil, dokumentasi Nominatim menyebut respons `{"error":"Unable to geocode"}` (belum terpancing saat uji); kode harus menanganinya.
- `zoom=10` ≈ tingkat kota/kabupaten; `addressdetails=1` memberi blok `address`.

**RestSharp di NuGet** (data registrasi resmi `api.nuget.org`):

| Versi | Rilis | Celah keamanan tercatat | Mendukung .NET 8 |
|---|---|---|---|
| 106.11.4 (saran modul) | 2020-05-18 | **Ada**: GHSA-9pq7-rcxv-47vq (CVE-2021-27293, ReDoS) | Lewat .NET Standard 2.0 |
| 106.12.0 | 2021-07-15 | – | Lewat .NET Standard 2.0 |
| 111.4.1 | 2024-07-30 | **Ada**: GHSA-4rr6-2v9v-wcpc (CVE-2024-45302, injeksi header) | Ya |
| **112.1.0** | 2024-10-04 | – | Ya (`net8.0`) |
| 114.1.0 (terbaru) | 2026-10-21 | – | Ya (`net8.0`) |

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

### Versi RestSharp (WEB-00)
Modul 10 menyarankan **RestSharp 106.11.4 + Newtonsoft.Json 12.0.3** untuk proyek **.NET Framework 4.7.2**. PyroSnap memakai **.NET 8**. Pertimbangan:
- API RestSharp versi 107 ke atas berbeda dari contoh lab (mis. `IRestResponse`/`client.Execute` di 106 vs pola async di versi baru).
- Sepengetahuan saya, RestSharp **sebelum 106.12.0** terkena kerentanan ReDoS (CVE-2021-27293) — **perlu diverifikasi** sebelum memilih 106.11.4.
- ✅ Terverifikasi (§3a): 106.11.4 memang memiliki celah GHSA-9pq7-rcxv-47vq, jadi **tidak dipakai**.
- **Usulan: RestSharp 112.1.0** — tanpa celah tercatat, target `net8.0` langsung, sudah stabil ±2 tahun, dan bentuk API-nya (`RestClient`, `RestRequest`, `ExecuteAsync`) sudah dikenal. 114.1.0 juga bersih tetapi baru sebulan dan mungkin mengubah API; bisa dinaikkan nanti. Kode tetap mengikuti struktur lab (kelas statis + method per layanan).

## 6. Checklist target

- [x] **WEB-00** — Kerentanan 106.11.4 terverifikasi; **RestSharp 112.1.0 dipakai** (disetujui 2026-11-23).
- [x] **WEB-01** — Ketiga endpoint dipanggil langsung; parameter `current` dan bentuk respons `jsonv2` dicatat di §3a (2026-11-23).
- [x] **WEB-02** — `KualitasUdaraApi` + model + cache 1 jam + timeout 10 detik.
- [x] **WEB-03** — `CuacaApi` + model + cache + timeout; arah mata angin 8 arah + perkiraan arah asap (+180°); angin < 2 km/jam = "tenang".
- [x] **WEB-04** — `WilayahApi` + User-Agent identitas + antrean jeda ≥ 1,1 detik + cache sampai aplikasi ditutup + berhenti 10 menit bila dibatasi (429/403); hanya saat Detail dibuka.
- [x] **WEB-05** — Kartu "Kondisi sekitar" di Detail (wilayah, kualitas udara + chip kategori, angin + arah asap, kelembapan/suhu/hujan, jam data, tombol muat ulang).
- [x] **WEB-06** — Kotak "Kualitas udara di <wilayah>" di kolom Wilayah saya (Publik/Tamu), kategori + saran bahasa awam.
- [x] **WEB-07** — Atribusi bertautan "Weather data by Open-Meteo.com" (CC BY 4.0) dan "© OpenStreetMap contributors" di Detail; Open-Meteo di kotak warga.
- [x] **WEB-08** — Gagal tidak melempar ke UI: host mati, timeout 10 detik, HTTP 400/403/429, JSON tak dikenal, data null → "Tidak tersedia" + pesan singkat (diuji).
- [ ] **WEB-09** — Kasus uji Black Box bagian F di `docs/debug/blackbox-testing.md` dijalankan di Windows.
- [ ] **WEB-10** — Panduan pemakaian (MVP-16) menjelaskan data tambahan dan sumbernya.
- [ ] **WEB-11** — (Opsional, keputusan) simpan nama wilayah ke database untuk menyelesaikan K2 secara permanen.

## 6a. Implementasi (2026-11-23)

| Berkas | Isi |
|---|---|
| `Models/KondisiSekitar.cs` | `HasilApi<T>` (data atau pesan gagal), `KualitasUdara`, `KondisiCuaca`, `NamaWilayah` (`Ringkas` = desa, kecamatan, kota, provinsi) |
| `Services/LayananWeb.cs` | `RestClient` (timeout 10 detik, User-Agent PyroSnap), `KirimAsync` (penggolongan gagal), angka URL selalu bertitik desimal, `CacheMemori<T>` |
| `Services/KualitasUdaraApi.cs` | Open-Meteo Air Quality: `pm2_5,pm10,us_aqi`; `Kategori(aqi)` → 6 kategori EPA berbahasa Indonesia + saran |
| `Services/CuacaApi.cs` | Open-Meteo Forecast: angin, arah, kelembapan, suhu, hujan; `ArahMataAngin`, `TeksArah` |
| `Services/WilayahApi.cs` | Nominatim `reverse` (`zoom=14`, `Accept-Language: id`), antrean + jeda + cache + berhenti bila dibatasi |
| `Views/DetailView.Kondisi.cs` + `DetailView.xaml` | Kartu "Kondisi sekitar" (WEB-05, WEB-07) |
| `Views/DashboardView.Warga.cs` + `DashboardView.xaml` | Kotak kualitas udara warga (WEB-06) |

Uji (program sementara, Linux, culture `id-ID`): 25/25 lulus — format angka URL, batas kategori AQI, 8 arah angin dan arah asap, pembacaan JSON (lengkap, sebagian, null, error, bukan JSON, Unable to geocode, titik laut), host mati, HTTP 400, timeout 10,0 detik, panggilan asli (Riau AQI 40 "Baik"; Jambi angin dari Barat Daya 224°; wilayah "Sialang Sakti, Tenayan Raya, Pekanbaru, Riau" dan "Solok Sipin, Jambi"), cache 0 ms, jeda Nominatim 1.525 ms.

## 7. Keputusan terbuka
1. Versi RestSharp: usulan 112.1.0 (WEB-00).
2. Nama wilayah disimpan ke database atau cukup cache memori (WEB-11, K2).
3. Indeks kualitas udara. **Sementara dipakai US AQI (kategori EPA, diterjemahkan)** karena tersedia langsung dan batasnya baku. Usulan lanjutan: PM2.5 dengan kategori ISPU (istilah resmi Indonesia, sudah disebut di Panduan) — **batas angka ISPU belum diverifikasi** dari peraturan KLHK, dan ISPU resmi memakai rata-rata 24 jam sehingga nilai per jam ditampilkan sebagai "perkiraan". Alternatif: `us_aqi` (sudah tersedia langsung di respons).

## 8. Rujukan
- Modul 10 — Minimum Viable Product (`docs/MODUL 10 - FIX.pdf`): bab Mengakses Layanan Pihak Ketiga, Lab 10.1, Tugas Kelompok
- [Open-Meteo Air Quality API — dokumentasi](https://open-meteo.com/en/docs/air-quality-api)
- [Open-Meteo — Licence (CC BY 4.0, atribusi)](https://open-meteo.com/en/licence)
- [Open-Meteo — Terms (batas tingkat gratis, non-komersial)](https://open-meteo.com/en/terms)
- [Open-Meteo — dokumentasi Forecast API](https://open-meteo.com/en/docs)
- [Nominatim Usage Policy (OSMF)](https://operations.osmfoundation.org/policies/nominatim/)
- [Nominatim — OpenStreetMap Wiki](https://wiki.openstreetmap.org/wiki/Nominatim)
- [Nominatim API — Reverse](https://nominatim.org/release-docs/latest/api/Reverse/)
- [RestSharp di NuGet](https://www.nuget.org/packages/RestSharp) dan advisori [GHSA-9pq7-rcxv-47vq](https://github.com/advisories/GHSA-9pq7-rcxv-47vq), [GHSA-4rr6-2v9v-wcpc](https://github.com/advisories/GHSA-4rr6-2v9v-wcpc)
