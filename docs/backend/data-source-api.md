# Backend: Sumber Data NASA FIRMS (HotspotDataSource)

> Status: **rencana**. Kode belum diubah. Disusun 2026-11-22.
> Tidak ada API key di dokumen ini; key hanya di konfigurasi lokal (`AppConfig`, lihat DBA-01 / ARCH-05).

## 1. Tujuan

`HotspotDataSource` mengambil data hotspot dari NASA FIRMS, menghasilkan `Hotspot` yang **benar**, dan gagal dengan jelas. Mencakup use case diagram `fetchData()` dan `sync()`.

## 2. Kondisi saat ini

- `FetchDataAsync()` memanggil URL FIRMS (VIIRS_SNPP_NRT, bbox Indonesia `95,-11,141,6`, rentang 1 hari) lewat `HttpClient` statis tanpa timeout kustom (`HotspotDataSource.cs:18,31`).
- URL dibangun di `DashboardView.xaml.cs:29-31` dengan **key hardcoded** (BUG-01); `HotspotDataSource` hanya menerima URL jadi.
- Parser `ParseCsvToHotspots` memakai indeks kolom tetap dan beberapa di antaranya diduga salah (BUG-03), waktu diisi `DateTime.Now` (BUG-04), `double.Parse` tanpa validasi (BUG-06).
- Setiap error → daftar berisi 1 titik dummy tanpa pemberitahuan (BUG-05).
- `SyncAsync()` selalu mengembalikan `true` karena daftar tak pernah kosong; `WaktuSinkronisasi` terisi walau datanya dummy.
- Tidak ada penyimpanan ke DB, tidak ada cache, tidak ada jadwal.

## 3. Kontrak endpoint (untuk dikonfirmasi)

Pola URL area FIRMS:
```
https://firms.modaps.eosdis.nasa.gov/api/area/csv/{KEY}/{SUMBER}/{barat,selatan,timur,utara}/{JUMLAH_HARI}
```
- `{KEY}` dari konfigurasi lokal; `{SUMBER}` saat ini `VIIRS_SNPP_NRT`; bbox Indonesia `95,-11,141,6`; hari = 1.
- FIRMS membatasi transaksi per key dalam jangka waktu tertentu; **cek dokumentasi resmi FIRMS** sebelum menambah auto-refresh (ADD-06) agar tidak menghabiskan kuota.

## 4. Pemetaan kolom CSV → `Hotspot`

> **Dikonfirmasi dengan data asli 2026-11-22** (header `VIIRS_NOAA20_NRT`, 2.261 baris, 14 kolom tiap baris). Catatan dari data asli:
> - `confidence` berisi `l`/`n`/`h` (contoh 5 hari: 134 l, 2.108 n, 19 h).
> - `acq_time` **tanpa nol di depan** (`338` = 03:38 UTC).
> - Baris diakhiri `\n` tanpa `\r`; respons yang hanya berisi header tidak punya baris baru di akhir.
> - `VIIRS_SNPP_NRT` berhenti per 2026-11-02 (BUG-19); default kini `VIIRS_NOAA20_NRT`, bisa diganti `FIRMS_SOURCE`.
> - MODIS memakai kolom `brightness` dan confidence angka; parser mendukung keduanya.

| Indeks | Kolom CSV | Properti `Hotspot` | Perlakuan |
|---|---|---|---|
| 0 | `latitude` | `Latitude` | `double`, `InvariantCulture` |
| 1 | `longitude` | `Longitude` | idem |
| 2 | `bright_ti4` | `Brightness` | `double` (Kelvin) |
| 5 | `acq_date` | `WaktuAkuisisi` (tanggal) | `yyyy-MM-dd` |
| 6 | `acq_time` | `WaktuAkuisisi` (jam) | `HHmm` (UTC), isi nol di depan |
| 9 | `confidence` | `Confidence` | `l/n/h` → 20/60/90 (SCH-04, kelas LAPAN) |
| 12 | `frp` | `Frp` | `double` (MW) |
| lain | `scan, track, satellite, instrument, version, bright_ti5, daynight` | – | Tidak dipakai model sekarang |

Aturan: **baca baris header lalu cari kolom berdasarkan nama**, bukan indeks tetap, agar perubahan urutan kolom tidak diam-diam merusak data.

## 5. Perilaku target

1. `FetchDataAsync()` → `GET` dengan timeout (±20–30 dtk) dan `CancellationToken`; status non-2xx → exception/hasil gagal dengan pesan (termasuk 401/403 untuk key salah, 429/atas kuota).
2. Parsing: lewati baris rusak, hitung jumlah yang dilewati; kolom wajib hilang → gagal jelas.
3. `SyncAsync()` → `true` hanya bila data valid > 0 diterima; mengisi `Hotspots` dan `WaktuSinkronisasi`; bila gagal, **data lama dipertahankan** dan status gagal dikembalikan.
4. Tidak ada data dummy di jalur produksi. Data contoh hanya lewat tombol/mode eksplisit yang diberi label.
5. (F4) Setelah sync berhasil: upsert ke tabel `hotspot` dan perbarui `hotspot_data_source.waktu_sinkronisasi`.
6. Hak sync: hanya Admin/Analis (FT-01d, F3).

## 6. Checklist target

### Fase 0
- [ ] **BE-00** — Key FIRMS lama dirotate; key baru hanya di `.env`/env. *Selesai bila* `git grep` tidak menemukan key di working tree. (BUG-01)

### Fase 1
- [x] **BE-01** — Ambil 1 baris + header CSV asli; catat kolom aktual di §4 dan ubah label "belum dikonfirmasi". *Selesai bila* tabel §4 sesuai data asli. (TST-01)
- [x] **BE-02** — Parser memakai header untuk menemukan kolom. *Selesai bila* urutan kolom diacak pada CSV uji tetap terbaca benar.
- [x] **BE-03** — Confidence `l/n/h` terpetakan sesuai keputusan SCH-04. (BUG-03, TST-02)
- [x] **BE-04** — Waktu akuisisi dari `acq_date` + `acq_time` UTC. (BUG-04, TST-03)
- [x] **BE-05** — `TryParse` + lewati baris rusak + hitung yang dilewati. (BUG-06, TST-05/06)
- [x] **BE-06** — Hapus fallback dummy; kegagalan menghasilkan error yang tampil di UI. (BUG-05, TST-04)
- [x] **BE-07** — Timeout + `CancellationToken`; tombol Muat Ulang menonaktifkan diri saat memuat. (BUG-15, TST-07)
- [x] **BE-08** — URL dibangun dari template + key konfigurasi di satu tempat (bukan di view). *Selesai bila* `DashboardView` tidak lagi memuat konstanta URL/key.
- [x] **BE-09** — `SyncAsync()` mengembalikan status yang bermakna dan mempertahankan data lama saat gagal.
- [x] **BE-10** — Pesan error dibedakan: jaringan, key/otorisasi, kuota, format data. *Selesai bila* tiap kasus memberi pesan berbeda di UI.

### Fase 4
- [x] **BE-11** — Setelah sync sukses, data di-upsert ke DB dan `waktu_sinkronisasi` diperbarui. (DBA-04)

## 7. Keputusan terbuka
1. Pemetaan confidence (SCH-04): rekomendasi `l=30, n=60, h=90`.
2. Mendukung sumber lain (mis. MODIS dengan confidence numerik) atau hanya VIIRS? Bila hanya VIIRS, dokumentasikan.
3. Zona waktu tampilan: UTC saja atau waktu lokal pengguna (WIB)? Rekomendasi: simpan UTC, tampil lokal + tulis zona.
4. Sync otomatis (ADD-06) di luar lingkup sampai disetujui.

## 8. Rujukan
`PyroSnap/Services/HotspotDataSource.cs`, `docs/debug/bug-fix.md` (BUG-01…06, 15), `docs/database/schema-changes.md`, `docs/debug/testing-schema.md`
