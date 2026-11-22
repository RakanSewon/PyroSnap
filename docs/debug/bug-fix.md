# List Bug Yang Ditemukan

> Sumber: review statis kode 2026-11-22 (BUG-01…19) dan uji menyeluruh di Linux 2026-11-23 (BUG-20…30, semuanya sudah diperbaiki di kode). Aplikasi tidak dijalankan (WPF butuh Windows), jadi tiap bug punya langkah verifikasi untuk dicek di Windows.
> Prioritas: **P0** = data salah / rahasia bocor, kerjakan dulu · **P1** = fungsi rusak atau menyesatkan · **P2** = kualitas/UX.
> Fase mengacu pada `docs/progress/backend-progress.md`. Rencana uji ada di `docs/debug/testing-schema.md`.

## Ringkasan

| ID | P | Judul | Lokasi | Fase |
|---|---|---|---|---|
| BUG-01 | P0 | API key NASA hardcoded & ada di riwayat git | `Views/DashboardView.xaml.cs:29-31` | F0 |
| BUG-02 | P0 | Key tile CARTO tertanam di HTML | `Assets/map.html:28` | F0 |
| BUG-03 | P0 | Confidence salah kolom (selalu 50) | `Services/HotspotDataSource.cs:75-79` | F1 |
| BUG-04 | P0 | Waktu akuisisi diisi `DateTime.Now` | `Services/HotspotDataSource.cs:87` | F1 |
| BUG-05 | P0 | Fallback data dummy diam-diam | `HotspotDataSource.cs:39-47`, `DashboardView.xaml.cs:129-134` | F1 |
| BUG-06 | P1 | Parsing rapuh (`double.Parse`, tanpa validasi baris) | `HotspotDataSource.cs:60-83` | F1 |
| BUG-07 | P1 | Format angka mengikuti culture (CSV rusak di id-ID) | `Views/ExportView.xaml.cs:63`, `Models/Hotspot.cs:23-29` | F1 |
| BUG-08 | P1 | "Kembali" membuat dashboard baru (unduh ulang, filter hilang) | `DetailView.xaml.cs:41`, `ExportView.xaml.cs:107` | F1 |
| BUG-09 | P2 | Kolom DataGrid terurut sebagai teks | `Views/HotspotRow.cs:23-26` | – (UI) |
| BUG-10 | P1 | Login hardcoded + kondisi `||` redundan | `Views/LoginView.xaml.cs:14-15,35` | F3 |
| BUG-11 | P1 | CHECK `format` laporan tidak menerima TXT | `database/001_schema.sql:97-98` | F2 |
| BUG-12 | P1 | `.env` tidak dibaca; default koneksi tanpa password | `Services/DatabaseConnection.cs:9-11` | F2 |
| BUG-13 | P2 | Pencarian dan filter tidak komposable | `DashboardView.xaml.cs:205-262` | F1 |
| BUG-14 | P2 | Peta `fitBounds` ulang tiap filter; pilihan grid tak disorot di Leaflet | `Assets/map.html:86-89` | F1 |
| BUG-15 | P2 | `HttpClient` tanpa timeout kustom; `async void` tanpa try/catch | `HotspotDataSource.cs:18`, `DashboardView.xaml.cs:274` | F1 |
| BUG-16 | P2 | Error tulis file export tidak ditangani | `ExportView.xaml.cs:90,98` | F1 |
| BUG-17 | P2 | Alert & MapViewer berbasis `Console.WriteLine`; kode mati | `src/Alert.cs:15`, `src/MapViewer.cs` | F5 / ARCH-03 |
| BUG-18 | P2 | Label "Tingkat Keparahan" sebenarnya confidence deteksi | `Views/DetailView.xaml.cs:27` | – (UI) |
| BUG-19 | P0 | Satelit `VIIRS_SNPP_NRT` berhenti mengirim data sejak 2026-11-02 (ditemukan saat F1) | `HotspotDataSource` URL | F1 |
| BUG-20 | P0 | Seed akun uji gagal total di database baru: baris `analis_uji` berperan Analis ditolak CHECK setelah migrasi 008 | `database/seed-local*.sql` | Uji 2026-11-23 |
| BUG-21 | P2 | Pesan gagal muat menyebut tombol lama "Perbarui Data dari NASA FIRMS" | `Views/DashboardView.Data.cs:184` | Uji 2026-11-23 |
| BUG-22 | P2 | Banner warga menyuruh memilih wilayah "di panel kiri", padahal kini di kolom kanan | `Views/DashboardView.Warga.cs:95` | Uji 2026-11-23 |
| BUG-23 | P1 | Template TextBox mengunci scrollbar `Hidden`: pratinjau Ekspor (200 baris) dan catatan verifikasi tanpa scrollbar | `Themes/Gaya.xaml` (TextBox) | Uji 2026-11-23 |
| BUG-24 | P2 | Label slider dan nilainya sebaris tidak muat di kolom filter 290 px; label terpotong | `Views/DashboardView.xaml` (filter) | Uji 2026-11-23 |
| BUG-25 | P2 | Pada jendela minimum (1024 px), jumlah titik + tombol "Lihat detail" menimpa tab daftar | `Views/DashboardView.xaml` (kartu daftar) | Uji 2026-11-23 |
| BUG-26 | P2 | Tinggi minimum peta + daftar melebihi ruang pada jendela 640 px; bagian bawah terpotong | `Views/DashboardView.xaml` | Uji 2026-11-23 |
| BUG-27 | P2 | Chip kategori udara panjang menghimpit judul di Detail dan kotak warga | `DetailView.xaml`, `DashboardView.xaml` | Uji 2026-11-23 |
| BUG-28 | P2 | Ubin angka Detail (keyakinan, FRP, kecerahan) terpotong pada jendela sempit | `Views/DetailView.xaml` | Uji 2026-11-23 |
| BUG-29 | P2 | Subjudul bilah atas dan nama pengguna panjang tidak dipotong rapi | `MainWindow.xaml` | Uji 2026-11-23 |
| BUG-30 | P1 | WebView2 Dashboard lama tidak dibuang saat pindah halaman / ganti tema (proses browser menumpuk) | `Views/DashboardView.xaml.cs`, `.Peta.cs` | Uji 2026-11-23 |
| BUG-31 | P0 | Peta dasar kosong bertuliskan "API KEY REQUIRED": CARTO kini mewajibkan API key | `Assets/map.html` | Uji Windows 2026-11-23 |
| BUG-32 | P2 | Tab, segmen, chip, dan profil tampil sebagai elips, bukan pil | `Themes/Gaya.xaml` (`RadiusPil` 999) | Uji Windows 2026-11-23 |
| BUG-33 | P2 | Peta terlalu kecil di Dashboard (terdesak KPI, status, banner warga, dan daftar) | `Views/DashboardView.xaml` | Uji Windows 2026-11-23 |
| BUG-34 | P1 | Titik berkeyakinan tinggi tertutup titik Nominal/Rendah yang digambar sesudahnya | `Assets/map.html` (`setHotspots`) | Uji peta 2026-11-23 |
| BUG-35 | P2 | Zoom awal terlalu jauh pada peta setinggi ±300 px (tampak sebagian besar Asia) | `Assets/map.html` (`fitBounds pad 0.3`) | Uji peta 2026-11-23 |
| BUG-36 | P1 | Legenda di kanan bawah menutupi Papua selatan: 166 titik termasuk semua titik Tinggi tersembunyi | `Assets/map.html` (legenda) | Uji peta 2026-11-23 |

---

## Detail

### BUG-01 (P0) — API key NASA hardcoded & bocor di git
- **Gejala:** konstanta `MapKey` berisi key FIRMS di kode; key ada di commit `6511572` dan `51be938`. Repo di GitHub.
- **Dampak:** siapa pun yang punya akses repo dapat memakai kuota key.
- **Rencana:** (1) rotate key di akun NASA, (2) baca key dari `AppConfig` (env/`.env`), (3) hapus konstanta, (4) bangun URL dari template tanpa key.
- **Verifikasi:** `git grep` tidak menemukan pola key di working tree; app tetap mengambil data memakai key baru dari `.env`.
- [ ] Key lama dirotate  [x] Key keluar dari kode  [ ] Terverifikasi
- *2026-11-22:* key dibaca dari `FIRMS_MAP_KEY` lewat `Services/AppConfig.cs`; konstanta dihapus dari `DashboardView`. Bila key belum diatur, dashboard menampilkan pesan dan data contoh. **Rotate key masih harus dilakukan pemilik akun NASA.** Belum diuji di Windows.

> Catatan: menghapus key dari riwayat git bersifat opsional dan merusak riwayat; rotate key lebih penting.

### BUG-02 (P0) — Key CARTO di `map.html`
- **Gejala:** query `?key=...` pada URL tile CARTO.
- **Rencana:** pakai tile tanpa key / kirim key lewat C# saat inisialisasi, atau ganti penyedia; rotate bila key itu pribadi.
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* key dibaca dari `CARTO_KEY` (opsional) dan disuntikkan ke halaman lewat `AddScriptToExecuteOnDocumentCreatedAsync` sebagai `window.PYROSNAP_CONFIG`; tanpa key, URL tile dipakai tanpa `?key=`. Belum diuji di Windows. Key lama tetap ada di riwayat git (commit `3b57db2`).

### BUG-03 (P0) — Confidence selalu 50 pada data live
- **Gejala:** parser membaca `kolom[10]` sebagai confidence dengan `int.TryParse`. Menurut skema CSV VIIRS yang terdokumentasi FIRMS, urutan kolom: `latitude, longitude, bright_ti4, scan, track, acq_date, acq_time, satellite, instrument, confidence, version, bright_ti5, frp, daynight`. Berarti indeks 10 = `version` (bukan angka) dan confidence ada di indeks 9 berisi `l`/`n`/`h`.
- **Dampak:** semua titik live bernilai 50 → filter dan warna peta tidak bermakna.
- **Rencana:** **ambil 1 baris CSV asli dulu** untuk memastikan urutan kolom (pada baris header, cari kolom berdasar nama, bukan indeks tetap); petakan `l/n/h` → angka (keputusan SCH-04).
- **Verifikasi:** hasil parse suatu baris asli: confidence sesuai huruf asli (l/n/h) → nilai terpetakan.
- [x] Skema kolom dikonfirmasi dari data asli  [x] Diperbaiki  [x] Terverifikasi (uji logika)
- *2026-11-22:* header asli FIRMS dikonfirmasi: confidence di indeks 9 berisi `l`/`n`/`h`. Parser kini mencari kolom dari header; pemetaan l=30, n=60, h=90 (SCH-04 opsi A), lalu `l` diubah ke 20 (2026-11-22) agar masuk kelas Rendah LAPAN. Diuji pada 2.261 baris asli NOAA-20: 134/2.108/19 sesuai jumlah l/n/h.

### BUG-04 (P0) — Waktu akuisisi = waktu sekarang
- **Gejala:** `new Hotspot(..., DateTime.Now)`; CSV punya `acq_date` + `acq_time` (HHMM, UTC).
- **Rencana:** gabungkan `acq_date` + `acq_time` jadi `DateTime` UTC; simpan UTC, tampilkan lokal di UI.
- **Verifikasi:** waktu hotspot = waktu di CSV asli, bukan waktu sync.
- [x] Diperbaiki  [x] Terverifikasi (uji logika)
- *2026-11-22:* `acq_time` dari FIRMS **tidak diberi nol di depan** (`338` = 03:38), jadi dipad ke 4 digit. Waktu disimpan `DateTimeKind.Utc`; Detail menampilkan waktu lokal + UTC.

### BUG-05 (P0) — Data dummy tersembunyi
- **Gejala:** `FetchDataAsync` menangkap semua exception dan mengembalikan 1 titik dummy (Riau, 12 Sep 2026). Karena daftar tak pernah kosong, cabang "API tidak tersedia" di dashboard (`DashboardView.xaml.cs:129-134`) tidak pernah jalan, sehingga pengguna melihat data palsu tanpa peringatan.
- **Rencana:** `FetchDataAsync` melempar exception / mengembalikan hasil gagal; UI menampilkan pesan dan, bila pakai data contoh, memberi label jelas.
- **Verifikasi:** matikan internet → pesan error tampil, tidak ada titik palsu tanpa label.
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* data dummy dihapus dari `HotspotDataSource` dan `DashboardView`. `SyncAsync()` mengembalikan `false` + `PesanGagal` dan mempertahankan data lama. Dashboard menampilkan pesan di bawah judul daftar; bila belum ada data sama sekali juga muncul dialog (overlay WPF tertutup peta WebView2). Sudah diuji di level logika (host mati, key salah); tampilan di Windows belum.

### BUG-06 (P1) — Parsing rapuh
- **Gejala:** `double.Parse` melempar exception pada satu baris rusak sehingga seluruh data gagal lalu jatuh ke dummy. Pemisahan dengan `Split(',')` rentan bila ada koma dalam kutipan. Baris berakhir `\r` (saat ini tak memengaruhi karena kolom terakhir tidak diparsing, tapi rentan bila kelak dipakai).
- **Rencana:** `TryParse` + lewati baris rusak dan hitung jumlah yang dilewati; baca header untuk indeks kolom.
- **Verifikasi:** CSV berisi 1 baris rusak → baris lain tetap terbaca, jumlah dilewati tercatat.
- [x] Diperbaiki  [x] Terverifikasi (uji logika)
- *2026-11-22:* `TryParse` di semua kolom; baris rusak/pendek/confidence asing dilewati dan dihitung (`JumlahBarisDilewati`, tampil di status). Respons bukan CSV (mis. "Invalid MAP_KEY.") → pesan "format tidak dikenali".

### BUG-07 (P1) — Culture pada angka
- **Gejala:** `ExportView.BuatCsv()` menyisipkan `{h.Latitude}` dst. memakai culture sistem. Di Windows locale Indonesia desimal berupa koma (`0,5071`) sehingga kolom CSV pecah. `Hotspot.GetDetail()` punya masalah yang sama (hanya kosmetik).
- **Rencana:** `CultureInfo.InvariantCulture` untuk semua format angka di export dan `GetDetail()`.
- **Verifikasi:** export dengan locale id-ID → 6 kolom per baris, desimal titik.
- [x] Diperbaiki  [x] Terverifikasi (uji logika, culture id-ID)
- *2026-11-22:* CSV dipindah ke `HotspotReport.Generate()` dengan `InvariantCulture`; `GetDetail()` juga. Belum dicoba dibuka di Excel.

### BUG-08 (P1) — Navigasi membuat dashboard baru
- **Gejala:** `Navigate(new DashboardView())` dipanggil dari Detail, Export, dan Login; `Loaded` memicu unduhan ulang dan menghapus filter/pencarian/pilihan.
- **Rencana:** simpan state (ARCH-07): reuse instance atau simpan data+filter di sesi.
- **Verifikasi:** filter confidence 80 → buka Detail → Kembali: filter dan daftar sama, tanpa request jaringan baru.
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* data, sumber, waktu sinkron, filter, area pencarian, dan hotspot terpilih disimpan di `AppSession`; dashboard baru memulihkannya tanpa unduh ulang. Logout memanggil `AppSession.Bersihkan()`.

### BUG-09 (P2, UI) — Sort kolom DataGrid
- **Gejala:** kolom terikat ke string ("85 %", "9.5 MW") → "100 %" terurut sebelum "85 %".
- **Rencana:** ikat kolom ke nilai numerik + `StringFormat`, atau set `SortMemberPath` ke properti numerik.
- *Dicatat di sini; dikerjakan bersama dokumen frontend (di luar fokus saat ini).*
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22 (FE1/UX-05):* kolom memakai `SortMemberPath` ke properti angka/waktu di `HotspotRow`; ditambah kolom Waktu dan Wilayah.

### BUG-10 (P1) — Login hardcoded
- **Gejala:** username/password demo ada di kode; `user.Login(...) || (username == DemoUser && password == DemoPass)` membuat hasil `User.Login` tak berarti; password dibandingkan plaintext.
- **Rencana:** lihat `docs/backend/auth-and-roles.md` (AUTH-xx).
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* akun demo dan kondisi `||` dihapus. Login mencari akun di tabel `users` (`UserRepository`, query berparameter) lalu `User.Login()` memverifikasi hash BCrypt (BCrypt.Net-Next 4.0.3). Uji logika 13/13 lulus; login nyata di Windows menunggu akun uji dibuat.

### BUG-11 (P1) — CHECK format laporan
- **Gejala:** `format IN ('PDF','CSV','XLSX')` tetapi aplikasi mengekspor TXT; menyimpan laporan TXT akan melanggar constraint.
- **Rencana:** SCH-03 (tambah `TXT`).
- [x] Diperbaiki  [x] Terverifikasi (tabel sementara)
- *2026-11-22:* migrasi `004_report_format_txt.sql`; TST-24 lulus (TXT diterima, format lain seperti DOC tetap ditolak). Berlaku di DB lokal setelah migrasi dijalankan sebagai `postgres`.

### BUG-12 (P1) — Konfigurasi DB
- **Gejala:** `.env` tidak dibaca; bila env `PYROSNAP_DB` kosong dipakai koneksi tanpa password (di Windows biasanya gagal tanpa pesan jelas).
- **Rencana:** DBA-01 (`AppConfig`); hapus default berkoneksi; pesan error jelas.
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* `DatabaseConnection.ConnectionString` membaca `PYROSNAP_DB` lewat `AppConfig` (env lalu `.env`) dan melempar `InvalidOperationException` berpesan jelas bila kosong. Parsing `.env` diuji dengan program uji sementara (10/10 lulus); koneksi ke PostgreSQL belum diuji.

### BUG-13 (P2) — Pencarian & filter tak komposable
- **Gejala:** `CariLokasi` selalu mulai dari `_semua` (menghapus filter slider); `BtnApplyFilter_Click` memakai `_tampil` bila ada area fokus sehingga filter bertumpuk tanpa bisa dilonggarkan.
- **Rencana:** ARCH-06 / FT-02a (satu `FilterCriteria`, hitung dari `_semua` setiap kali).
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* `TerapkanKriteria()` selalu menghitung dari data penuh: `FilterCriteria.IsMatch` lalu area pencarian. Area masih predikat terpisah karena keputusan `Wilayah` (K2) belum diambil.

### BUG-14 (P2) — Peta
- **Gejala:** `setHotspots` memanggil `fitBounds` setiap kali; pilihan di grid tidak menyorot marker pada Leaflet.
- **Rencana:** FT-03b, FT-03c.
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* `setHotspots(json, fit)` hanya zoom setelah data baru dimuat/Reset; `highlightHotspot(id)` menyorot titik terpilih dan menggeser peta bila titik di luar layar.

### BUG-15 (P2) — Timeout & `async void`
- **Gejala:** `HttpClient` default (100 dtk) membuat UI "memuat" lama; `BtnReload_Click` `async void` tanpa try/catch.
- **Rencana:** timeout ±20–30 dtk, `CancellationToken`, nonaktifkan tombol saat memuat.
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* timeout 30 dtk + pesan khusus; `FetchDataAsync`/`SyncAsync` menerima `CancellationToken`; tombol Muat Ulang nonaktif saat memuat; handler `async void` dibungkus try/catch.

### BUG-16 (P2) — Error tulis file
- **Gejala:** `File.WriteAllText` tanpa try/catch (file terkunci Excel, tanpa izin → crash).
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* `ExportView` menangkap `IOException`/`UnauthorizedAccessException` dan menampilkan pesan. Uji file terkunci Excel butuh Windows.

### BUG-17 (P2) — Alert/MapViewer konsol & kode mati
- **Gejala:** keluaran `Console.WriteLine` tidak terlihat di WPF; `MapViewer` tidak dipakai.
- **Rencana:** ARCH-03, FT-07c.
- [ ] Diperbaiki  [ ] Terverifikasi
- *2026-11-22:* bagian Alert selesai — `Alert.SendAlert()` tidak lagi memakai `Console.WriteLine`; alert disimpan di tabel `alert` dan tampil di halaman Alert. `MapViewer.cs` masih berbasis konsol dan menunggu keputusan K1 (ARCH-03).

### BUG-18 (P2, UI) — Label menyesatkan
- **Gejala:** "Tingkat Keparahan" diturunkan dari confidence (keyakinan deteksi satelit), bukan tingkat bahaya.
- **Rencana:** FT-04b.
- [x] Diperbaiki  [ ] Terverifikasi
- *2026-11-22 (FE1/UX-04):* Detail menampilkan "Keyakinan deteksi: {kelas LAPAN} ({n}%)" + penjelasan; teks berwarna gelap, warna tingkat hanya di titik.

### BUG-19 (P0) — Sumber satelit berhenti
- **Gejala:** API `data_availability` FIRMS menunjukkan `VIIRS_SNPP_NRT` hanya punya data sampai 2026-11-02; permintaan 1 dan 5 hari untuk Indonesia mengembalikan 0 baris. Akibatnya aplikasi selalu jatuh ke data dummy.
- **Perbaikan:** default sumber diganti `VIIRS_NOAA20_NRT` (format kolom sama, 450 titik/hari saat diuji); dapat diganti lewat `FIRMS_SOURCE` di `.env` (mis. `VIIRS_NOAA21_NRT`).
- [x] Diperbaiki  [x] Terverifikasi (uji live: 451 titik)

### BUG-20 (P0) — Seed akun uji gagal di database baru
- **Gejala:** di database kosong (alur Windows: migrasi 001–009 lalu seed), seed berhenti dengan `violates check constraint … Failing row contains (… analis_uji … Analis)`. Karena seed satu transaksi, **tidak ada akun yang terbuat** sehingga tidak ada yang bisa login.
- **Penyebab:** migrasi 008 menggabungkan Analis ke Admin dan CHECK hanya mengizinkan Admin/Operator/Publik, tetapi seed masih membuat `analis_uji` berperan Analis. Database Linux lama tidak terkena karena seed dijalankan sebelum 008.
- **Perbaikan:** baris itu diganti `publik_uji` berperan Publik (password tetap) di `seed-local.sql` dan `seed-local.example.sql`; `docs/database/acount-testing.md` diperbarui.
- **Verifikasi:** migrasi 001–009 + seed di skema sementara `pg_temp`: seed baru menghasilkan `admin_uji:Admin, operator_uji:Operator, publik_uji:Publik`; seed lama terbukti gagal; seed dua kali tidak menggandakan; penanda `<password-…>` ditolak (6/6).
- [x] Diperbaiki  [x] Terverifikasi (Linux)  [ ] Dicek di Windows

### BUG-21 (P2) — Nama tombol lama di pesan
- **Perbaikan:** "Perbarui Data dari NASA FIRMS" → "Perbarui dari NASA FIRMS" (sesuai tombol D5).
- [x] Diperbaiki  [ ] Dicek di Windows

### BUG-22 (P2) — "Panel kiri" di banner warga
- **Perbaikan:** teks menunjuk bagian "Wilayah saya" (kolom kanan) dan menyebut kualitas udara.
- [x] Diperbaiki  [ ] Dicek di Windows

### BUG-23 (P1) — TextBox tanpa scrollbar
- **Gejala:** pratinjau Ekspor berisi sampai 200 baris dan catatan verifikasi multibaris tidak menampilkan scrollbar walau diatur `Auto`; isi panjang hanya bisa digulir dengan roda tetikus/keyboard.
- **Penyebab:** template TextBox baru (DS-04/07) menulis `HorizontalScrollBarVisibility="Hidden"` dan `VerticalScrollBarVisibility="Hidden"` tetap pada `PART_ContentHost`.
- **Perbaikan:** kedua nilai memakai `TemplateBinding` dari TextBox (bawaan TextBox tetap Hidden).
- [x] Diperbaiki  [ ] Dicek di Windows (Ekspor: scrollbar tampil)

### BUG-24 … BUG-29 (P2) — Tata letak pada jendela kecil
Dihitung dari ukuran jendela minimum 1024 × 640 (area halaman ±736 × ±420 px setelah menu samping dan bilah atas):
- **BUG-24:** "Keyakinan deteksi minimum" + "80 % (Tinggi ke atas)" ±300 px pada ruang ±250 px → label dan nilai dipisah baris (nilai di bawah slider, rata kanan).
- **BUG-25:** tab (±320 px) + jumlah titik + tombol (±220 px) pada kartu ±400 px → jumlah dan tombol "Lihat detail" pindah ke baris bawah kartu.
- **BUG-26:** `MinHeight` peta 160 + daftar 150 + jarak 16 > ruang tersisa ±284 px → 110 + 120 + 12.
- **BUG-27:** chip "Tidak sehat bagi kelompok sensitif · AQI …" (±250 px) di samping judul pada kolom ±240 px → chip di baris sendiri dan boleh turun baris.
- **BUG-28:** ubin angka Detail selebar ±90 px di jendela minimum → label dan angka boleh turun baris.
- **BUG-29:** subjudul bilah atas (mis. halaman Peringatan) dan nama pengguna hingga 30 karakter → dipotong dengan "…", subjudul lengkap di tooltip, nama maks. 170 px.
- [x] Diperbaiki  [ ] Dicek di Windows pada ukuran minimum dan 1280 × 760

### BUG-30 (P1) — WebView2 tidak dibuang
- **Gejala (potensi):** setiap kembali ke Dashboard (dari Detail, Ekspor, menu, atau ganti tema) dibuat `DashboardView` + WebView2 baru; yang lama tidak di-`Dispose`, sehingga proses browser Edge WebView2 bisa menumpuk dan aplikasi makin berat.
- **Perbaikan:** `Unloaded` → tandai `_petaDibuang` lalu `MapWeb.Dispose()`; semua panggilan skrip peta lewat `PetaSiap`, sehingga pemuatan data yang selesai setelah halaman ditinggalkan tidak menyentuh WebView2 yang sudah dibuang.
- [x] Diperbaiki  [ ] Dicek di Windows (Pengelola Tugas: jumlah proses `msedgewebview2` tidak terus bertambah setelah bolak-balik Dashboard ↔ Detail)

### BUG-31 (P0) — Peta "API KEY REQUIRED"
- **Gejala (Windows, 2026-11-23):** layer "Peta" hanya berisi tulisan miring "API KEY REQUIRED · carto.com/basemaps/apikey"; layer Satelit normal.
- **Penyebab:** CARTO Basemaps kini mewajibkan API key. Tanpa key, server tetap menjawab HTTP 200 tetapi setiap tile berisi watermark (dicek dengan `curl`: tile 2 KB bertuliskan "API KEY REQUIRED").
- **Perbaikan:** peta dasar bawaan diganti **Esri Light Gray Canvas** (tema terang) / **Dark Gray Canvas** (tema gelap) + lapisan nama tempat (`…_Reference`), tanpa key, penyedia sama dengan layer Satelit; atribusi sesuai `copyrightText` layanan ("Esri, HERE, Garmin, © OpenStreetMap contributors, and the GIS user community"). CARTO hanya dipakai bila `CARTO_KEY` diisi di `.env`.
- **Verifikasi:** keempat URL tile Esri dicoba (`curl`: HTTP 200, gambar bersih terang & gelap); uji `map.html` 4 mode lulus (tanpa konfigurasi, terang, gelap, dengan `CARTO_KEY`).
- [x] Diperbaiki  [x] Terverifikasi (Linux)  [ ] Dicek di Windows

### BUG-32 (P2) — Pil jadi elips
- **Penyebab:** `RadiusPil` = 999. WPF tidak membatasi `CornerRadius` ke setengah tinggi, sehingga sudut raksasa membentuk elips.
- **Perbaikan (sesuai permintaan tim: persegi bersudut bulat):** `RadiusPil` 10 (tab), `RadiusChip` 6 (chip, pil KPI), wadah segmen 12 + segmen 8, profil 12. Lingkaran asli (logo, avatar, tombol ikon) tetap karena radiusnya tepat setengah ukuran.
- [x] Diperbaiki  [ ] Dicek di Windows

### BUG-33 (P2) — Peta terlalu kecil
- **Perbaikan:** pembagian tinggi peta : daftar 5 : 4 → **2 : 1**; kartu KPI lebih ringkas (padding 16,10, angka 24 px); banner warga dipindah dari baris sendiri ke kolom kanan di atas "Wilayah saya".
- [x] Diperbaiki  [ ] Dicek di Windows (1280 × 760 dan ukuran minimum)

### BUG-34 … BUG-36 — Titik api di peta (uji 2026-11-23)
**Cara uji:** data asli NASA FIRMS (VIIRS NOAA-20, 22 Nov 2026: 691 titik — 4 Tinggi, 617 Nominal, 70 Rendah) diubah seperti `HotspotDataSource` + `KirimDataKePeta`, lalu dikirim ke `map.html` yang sama lewat `setHotspots(json, true)` di browser, dengan konfigurasi tema terang/gelap yang disuntikkan seperti aplikasi. Ukuran peta disimulasikan 660 × 320 px (perkiraan di Dashboard).

Hasil yang sudah benar: ke-691 penanda tampil di seluruh Indonesia; tiga tingkat terbedakan (ukuran 9/7/5, warna, tepi); tooltip berisi koordinat, kelas keyakinan, FRP; sorotan titik terpilih jelas; tema gelap memakai tile gelap dan legenda gelap.

- **BUG-34 (P1):** keempat titik Tinggi tertutup 11–73 penanda lain (urutan gambar = urutan data). **Perbaikan:** `naikkanTinggi()` — titik Tinggi (dan titik terpilih) dibawa ke lapisan teratas setelah data dimuat dan setelah sorotan dilepas. **Verifikasi:** posisi lapisan titik Tinggi 687–690 dari 691, tetap setelah pilih/lepas sorotan.
- **BUG-35 (P2):** `fitBounds(… pad(0.3))` pada 660 × 320 px → zoom 3. **Perbaikan:** `pad(0.05)` → zoom 4, semua titik tetap di dalam peta (0 di luar layar).
- **BUG-36 (P1):** legenda kanan bawah menutupi 166 titik di Papua selatan (Merauke), termasuk 4/4 titik Tinggi. **Perbaikan:** legenda pindah ke kiri bawah (Samudra Hindia). **Verifikasi:** 0 titik tertutup legenda, atribusi, kontrol layer, maupun tombol zoom.
- Uji `map.html` (Node) 4 mode tetap lulus.
- [x] Diperbaiki  [x] Terverifikasi (browser, data asli)  [ ] Dicek di aplikasi Windows

---

# Bug yang Diselesasikan

> Kosong. BUG-03/04/06/07/19 sudah lulus uji logika, tetapi baru dipindah ke sini setelah dicek juga di aplikasi (Windows). Pindahkan entri dari daftar di atas ke tabel ini setelah *diperbaiki dan terverifikasi*.

| ID | Tanggal | Commit | Ringkasan perbaikan | Cara verifikasi | Verifikator |
|---|---|---|---|---|---|
| – | – | – | – | – | – |
