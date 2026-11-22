# To-Do List PyroSnap — rangkuman sesi 2026-11-22 untuk dilanjutkan di sesi berikutnya

> Dibuat di akhir sesi, supaya sesi berikutnya bisa langsung lanjut tanpa membaca ulang seluruh percakapan.
> Dokumen terkait: `docs/MVP.md` (target akhir), `docs/progress/backend-progress.md`, `docs/progress/frontend-progress.md`.
> **Cara memulai sesi berikutnya:** minta Claude membaca `docs/to-do-list.md` dan `docs/MVP.md` lebih dulu, lalu pilih butir dari bagian 3.

## 1. Posisi saat ini

- **Branch:** `545573`. **Commit terakhir:** `484528a` ("feat: backend, database, alert, peran, dan perbaikan frontend PyroSnap", 85 file). **Belum di-push** (lokal "ahead 2" dari `origin/545573`: commit kita dan merge PR #9 yang sudah ada sebelumnya).
- **Aturan commit:** pesan commit/PR **tanpa** trailer atau atribusi Claude/Anthropic (keinginan tim: Claude tidak boleh tercatat sebagai kontributor). Sudah disimpan di memori proyek.
- **Tidak ikut commit (sengaja):** `.serena/`, `Claude.md`, `Diagram_Pyro_Snap.drawio.png`, `log.md` (sudah untracked sebelum sesi), serta `.env` dan `database/seed-local.sql` (di-gitignore, berisi rahasia).
- **Database lokal** (PostgreSQL 18.6, user aplikasi `pyrosnap_app`, bukan pemilik tabel): migrasi `000`–`007` sudah dijalankan. **`008` dan `009` BELUM dijalankan** (per pengecekan terakhir: constraint peran masih `Admin/Analis/Operator`, tabel `hotspot_verifikasi` belum ada). Isi tabel: 3 akun uji, **0 hotspot, 0 alert, 0 laporan** → aplikasi belum pernah menyimpan data ke DB ini.
- **Pengujian:** semua uji yang pernah dijalankan hanyalah **uji logika di Linux** (program uji sementara di folder kerja sesi, **tidak ada di repo**). **Belum ada uji di Windows** sama sekali.

## 2. Yang sudah selesai (ringkas)

| Bagian | Hasil |
|---|---|
| Perencanaan | 28 berkas dokumen di `docs/` (arsitektur, fitur, debug, database, backend, frontend, UI/UX) dengan checklist dan ID tetap |
| F0 Keamanan | `AppConfig` (env + `.env`); key FIRMS/CARTO keluar dari kode |
| F1 Data | Parser FIRMS berbasis header; waktu UTC; data dummy dihapus; sumber default `VIIRS_NOAA20_NRT` (SNPP berhenti 2026-11-02) |
| F2 Database | Migrasi bernomor `000`–`009`, `migrate.sh`, status DB di halaman Login |
| F3 Login | Login via tabel `users` (BCrypt); akun demo dihapus |
| F4 Persistensi | Hotspot di-upsert tanpa duplikat; laporan tercatat bersama hotspotnya dalam satu transaksi |
| F5 Alert | Ambang kelas tinggi LAPAN (confidence ≥ 80); batas 50 per sinkron; halaman Peringatan |
| F7 Peran | Admin (gabungan Admin+Analis), Operator, Publik, Tamu; daftar mandiri; verifikasi lapangan; wilayah pilihan; data historis; Panduan & Kontak Darurat |
| FE0–FE3 | Dashboard dipecah 6 partial class; helper `TingkatHotspot`/`Navigasi`/`PesanUi`; kelas tingkat LAPAN; bilah status; ringkasan per wilayah; teks ≥ 12 px; istilah diseragamkan |

## 3. To-do (urut prioritas)

### A. Hal yang perlu dilakukan pengguna lebih dulu (Claude tidak bisa)
- [ ] **Push** commit `484528a` ke GitHub (bila sudah yakin).
- [ ] **Jalankan migrasi 008 dan 009** (butuh `postgres`):
  `cat database/008_*.sql database/009_*.sql | sudo -u postgres psql -v ON_ERROR_STOP=1 -d pyrosnap`
- [ ] **Rotasi key NASA FIRMS** (key lama ada di riwayat git/GitHub) dan putuskan soal key CARTO; isi key baru di `.env`.
- [ ] **Uji di Windows** (dibutuhkan .NET 8 SDK + WebView2 Runtime): daftar uji ada di `docs/database/acount-testing.md` §6 (ACC-02…ACC-17) dan `docs/progress/frontend-progress.md` (FE5). Kirim hasil per nomor (lulus/gagal + tangkapan layar/pesan error).
- [x] **Format Modul 10** sudah ada: `docs/MODUL 10 - FIX.pdf` (dipindahkan dari Downloads 2026-11-23). Hasilnya: worksheet `docs/debug/blackbox-testing.md` (64 kasus, format Lab 10.2) dan rencana `docs/backend/web-api.md`.
- [x] PDF Modul 10 **tidak** di-commit: sudah masuk `.gitignore` (2026-11-23). PDF tetap ada di `docs/` secara lokal.
- [ ] Baca password akun uji di `database/seed-local.sql` (jangan dibagikan lewat chat): `admin_uji`, `analis_uji` (jadi Admin setelah migrasi 008), `operator_uji`.
- [ ] Cek bentuk "Worksheet Homework Modul 10 – Minimum Viable Product" di kelas dan berkas tambahan yang diminta (MVP-36).

### B. Agenda sesi berikutnya
1. **Sistem desain baru** (FE4) — **diterapkan 2026-11-23** (lihat `docs/ui/design-system.md`): gaya **D5** mengikuti gambar referensi tim (menu samping, bilah atas dengan lonceng + profil, kartu KPI, segmen pil, baris terpilih pil gelap); tema **terang + gelap** dengan tombol di menu samping (pilihan disimpan di `%AppData%\PyroSnap\settings.json`); ikon Segoe + logo nyala vektor; peta gelap (CARTO `dark_all`); tetap **.NET 8 LTS** dengan template kontrol sendiri (D6). DS-04…09 selesai di kode; build 0 error, pemeriksa kunci + 68 pasangan kontras + uji peta 3 mode lulus. **Sisa DS-10: cek di layar Windows** (semua halaman × peran × tema; glyph ikon di Windows 10/11). Batasan: kalender DatePicker masih terang di tema gelap.
2. **Pekerjaan MVP** (`docs/MVP.md`): finalisasi, implementasi web API pihak ketiga (`docs/backend/web-api.md`, WEB-00…11), panduan, pengemasan, menjalankan worksheet Black Box (`docs/debug/blackbox-testing.md`), pengumpulan.

### C. Keputusan yang masih menunggu tim
- [x] **Lingkup Web API** — diputuskan 2026-11-23: mengonsumsi REST API pihak ketiga (bukan membangun server): kualitas udara + cuaca/angin (Open-Meteo) + nama wilayah (Nominatim/OSM), pustaka RestSharp. **Dikodekan 2026-11-23** dengan RestSharp 112.1.0: kartu "Kondisi sekitar" di Detail dan kotak kualitas udara warga; uji otomatis 25/25 lulus (termasuk panggilan asli). Sisa: uji bagian F di Windows (WEB-09), panduan (WEB-10), kategori ISPU (belum diverifikasi; sementara US AQI), simpan nama wilayah ke DB atau tidak (WEB-11).
- [ ] **K1** — nasib `PyroSnap/src/MapViewer.cs` (kode konsol yang tak terpakai): hapus atau hubungkan.
- [ ] **K2 (formal)** — atribut wilayah pada Hotspot. Sementara memakai 8 area pantau ±3° (`Services/WilayahPantau.cs`); batas provinsi/38 provinsi menunggu ADD-07.
- [ ] **K7** — apakah class repository dianggap tambahan di luar diagram.
- [ ] **K8** — fitur tambahan lain: air quality (ADD-01, sesuai README), skor risiko (ADD-02), statistik/tren, ekspor PDF/XLSX (ADD-05), auto-refresh (ADD-06), logging (ADD-08).
- [ ] **FK6** — Kelola Akun dan Riwayat Laporan untuk Admin (SCR-14/15; matriks sudah menjanjikan tetapi fiturnya belum ada).
- [ ] **CPY-01/02/03** — persetujuan tim atas glosarium, teks peringatan warga, dan **pengecekan isi Panduan & kontak darurat oleh anggota tim** (sumber: Kemenkes, Permenkominfo 10/2016 + liputan layanan 112, BNPB 117, daftar nomor darurat Katadata, LAPAN; dicek Claude 2026-11-22, belum dicek tim).
- [ ] **Fase 6** (MVVM/DI/interface/unit test formal) — tetap opsional; versi lengkapnya pernah ditolak sebagai over-engineering.
- [ ] Penguncian setelah gagal login berulang (AUTH-B2), jenis pembuatan akun Admin/Operator (lewat DB atau Kelola Akun).

### D. Pekerjaan teknis yang tersisa
- [ ] **ARCH-01…04** — pindahkan `User`/`Alert`/`HotspotReport` ke `Models/`, seragamkan namespace, hapus `MapViewer.cs` (K1), hapus folder `src/` di root; **ARCH-09/10** — aturan error lintas lapisan dan diagram alur data final.
- [ ] **FT-03d / MVP-05** — Leaflet dibundel lokal (sekarang dari CDN unpkg; tanpa internet peta kosong).
- [ ] **Sinkronkan centang dokumen** — FT-02d, FT-03a, FT-04a, FT-04b sudah terpenuhi di kode tetapi belum dicentang; lengkapi `progress/testing-progress.md` dan `report/*` (masih kosong).
- [ ] **TST-A1** — berkas data uji CSV di `docs/debug/samples/` (belum dibuat; aman: tanpa key).
- [ ] **Jadikan uji sementara sebagai uji resmi** (proyek uji di repo atau dokumentasi yang dapat ditelusuri) — untuk laporan Modul 10 (MVP-29).
- [ ] **SET-01/02, SCH-A3, SCH-A5** — kesepakatan versi PostgreSQL tim, uji setup oleh minimal 2 anggota, prosedur cadangan, perbarui ERD.
- [ ] **Skrip database Windows-friendly** — `migrate.sh` dan perintah `sudo -u postgres` tidak berjalan di Windows (MVP-22).
- [ ] **`.env.example`** belum ada (MVP-21).

## 4. Keputusan yang sudah diambil di sesi ini (jangan diulang)

| Topik | Keputusan |
|---|---|
| Arsitektur | Bertahap: ikuti 8 class diagram; code-behind dirapikan (partial class + helper), **bukan MVVM**; Fase 6 opsional |
| Database | Bertahap penuh: login, hotspot, laporan, alert, verifikasi, preferensi wilayah disimpan di PostgreSQL; query berparameter, tanpa ORM |
| Skema | Paket inti disetujui (SCH-01/02/03/06/07/08/09/10); SCH-05 (indeks koordinat) ditunda |
| Confidence VIIRS | `l=20`, `n=60`, `h=90` (opsi A; `l` diubah dari 30 agar masuk kelas Rendah LAPAN) |
| Hash password | **BCrypt** (`BCrypt.Net-Next` 4.0.3); akun uji di-hash lewat pgcrypto |
| Ambang alert | **Confidence ≥ 80** (kelas tinggi LAPAN, "segera penanggulangan"); FRP hanya informasi (tidak ada ambang standar FRP di literatur; angka 50 MW **bukan** dari jurnal) |
| Kelas tingkat di UI | Rendah < 30 · Nominal 30–79 · Tinggi ≥ 80 (Panduan LAPAN Fire Hotspot v2.0) |
| Peran | **Admin** (Admin+Analis digabung) · **Operator** · **Publik** (daftar mandiri, boleh ekspor) · **Tamu** (tanpa akun, tanpa ekspor) |
| Hak akses | Sinkron FIRMS: Admin saja. Peringatan operasional, verifikasi lapangan, navigasi ke titik, data historis: Admin & Operator. Ekspor: Admin, Operator, Publik (bukan Tamu). Banner peringatan warga + wilayah pilihan: Publik (tersimpan) dan Tamu (hanya sesi) |
| Penerima alert | **Mengganti keputusan awal "semua user"**: alert operasional hanya Admin & Operator |
| Operator | Posko & petugas lapangan; membaca data dari DB (tidak mengambil dari NASA) |
| Tampilan | Sistem desain: gaya D5 dashboard kartu sesuai gambar referensi (2026-11-23, menggantikan "rapikan tema api"); tingkat = warna + ukuran + tepi + label; Segoe UI + ikon Segoe Fluent/MDL2 (tanpa font bundel); mode terang/gelap; tetap .NET 8 LTS (template manual); filter tetap tombol "Terapkan" |
| Sumber data | Default `VIIRS_NOAA20_NRT` (bisa diganti `FIRMS_SOURCE`) |
| Wilayah (sementara) | 8 area pantau (±3°, pusat terdekat) |
| Panduan | Dapat dibuka tanpa login |

## 5. Catatan teknis untuk sesi berikutnya

- **Build di Linux:** `dotnet build PyroSnap/PyroSnap.csproj -p:EnableWindowsTargeting=true` (hasil terakhir: 0 error, 4 warning bawaan). UI WPF tidak bisa dijalankan di Linux.
- **Konfigurasi (`.env` di root repo, di-gitignore):** `PYROSNAP_DB`, `FIRMS_MAP_KEY`, opsional `FIRMS_SOURCE` dan `CARTO_KEY`. Environment variable menang atas `.env`. **Jangan mencetak nilai `.env` di chat.**
- **Database:** pemilik tabel adalah `postgres`; user aplikasi `pyrosnap_app` hanya DML. Migrasi harus dijalankan sebagai `postgres`. Uji yang menyentuh DB dilakukan di **tabel sementara** (`SET search_path = pg_temp`) agar DB asli tidak berubah.
- **Akun uji:** `admin_uji`, `analis_uji`, `operator_uji` (password di `database/seed-local.sql`, izin 600). Akun Publik dibuat lewat layar Daftar di aplikasi.
- **Data uji nyata:** CSV FIRMS NOAA-20 (2.261 baris, 134 `l` / 2.108 `n` / 19 `h`) pernah diambil ke folder kerja sesi; ambil ulang dengan key bila perlu (`area/csv/{KEY}/VIIRS_NOAA20_NRT/95,-11,141,6/5`).
- **Kontras warna terhitung (WCAG):** `MutedText` 5,17:1 ✅; putih di `FireRed` 6,57:1 ✅; warna tingkat lama (kuning 1,66 / oranye 2,59) ❌ → diganti palet gelap di `design-system.md` §4; batas input lama `#CCCCCC` 1,61:1 ❌ → `#8A8A8A` (3,45).
- **Alur data sekarang:** Admin → ambil NASA FIRMS → simpan DB → buat alert (conf ≥ 80) → tampil; bila FIRMS gagal → data terakhir dari DB. Operator/Publik/Tamu → baca dari DB (24 jam sebelum akuisisi terbaru).
- **Dokumen kosong:** `docs/progress/testing-progress.md`, `docs/report/{backend,database,frontend,final}-report.md`.
- **Pelajaran dari `log.md`:** implementasi yang melebihi permintaan pernah ditolak sebagai over-engineering → konfirmasi lingkup dulu untuk fitur tambahan (termasuk Web API).
- **Istilah:** "MVP" = *Minimum Viable Product* (judul Modul 10 juga "Minimum Viable Product").
- **Modul 10:** "layanan web api" = mengakses REST API pihak ketiga (Lab 10.1, RajaOngkir + RestSharp 106.11.4 untuk .NET Framework 4.7.2); pengujian = Black Box seperti Lab 10.2; pengumpulan ke Worksheet Homework Modul 10. **Jangan menyalin API key praktikum dari PDF ke repo.**

## 6. Rujukan
`docs/MVP.md`, `docs/progress/backend-progress.md`, `docs/progress/frontend-progress.md`, `docs/database/setup-and-seed.md`, `docs/database/acount-testing.md`, `docs/ui/design-system.md`, `docs/ui/ux/ux-audit.md`, `log.md`
