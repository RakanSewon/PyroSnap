# Worksheet Black Box Testing PyroSnap (format Lab 10.2, Modul 10)

> Format mengikuti **Lab 10.2 Functional Testing** dan contoh worksheet di Modul 10 (`docs/MODUL 10 - FIX.pdf`): Nama Aplikasi, Tanggal Pengujian, Tester; kolom **No · Halaman yang Diuji · Aksi Aktor · Reaksi Sistem (Benar / Salah) · Hasil (Sesuai / Tidak Sesuai)**, dikelompokkan per jenis pengguna.
> Jenis pengujian (istilah Modul 10): **functional testing teknik black box**, tingkat **system testing** (seluruh fitur end-to-end) dan bahan **acceptance testing** oleh anggota/pengguna. Uji logika yang pernah dijalankan di Linux (parser, repository, hak akses, peta) termasuk unit/integration testing dan dicatat terpisah di `testing-schema.md`.
> Status: **belum dijalankan** (butuh Windows). Baris bertanda ⏳ menunggu fiturnya selesai (Web API, kemasan). Isi kolom Hasil dengan tanda **✓** pada salah satu pilihan.

| | | | |
|---|---|---|---|
| **Nama Aplikasi:** | PyroSnap — Pemantauan Titik Panas | **Tanggal Pengujian:** | ____________ |
| | | **Tester:** | ____________ |

Persiapan: migrasi `000`–`009` sudah dijalankan; akun uji `admin_uji`, `operator_uji` (password di `database/seed-local.sql`); `.env` berisi `PYROSNAP_DB` dan `FIRMS_MAP_KEY`; komputer Windows dengan WebView2 Runtime dan koneksi internet (kecuali disebut lain).

## A. Halaman Umum (Masuk, Daftar, Tamu, Panduan)

| No. | Halaman yang Diuji | Aksi Aktor | Reaksi Sistem — Benar | Reaksi Sistem — Salah | Hasil |
|---|---|---|---|---|---|
| 1 | Masuk | Membuka aplikasi | Halaman Masuk tampil; tertulis "Database: Terhubung ke database 'pyrosnap' …" | Aplikasi error / status database tidak tampil | Sesuai ( ) Tidak Sesuai ( ) |
| 2 | Masuk | Memasukkan username dan password benar (`admin_uji`), tekan Enter | Masuk ke Dashboard; profil di bilah atas menampilkan "admin_uji" dan "Admin"; menu samping berisi Dashboard, Peringatan, Ekspor Laporan, Panduan | Tetap di halaman Masuk / pesan kesalahan | Sesuai ( ) Tidak Sesuai ( ) |
| 3 | Masuk | Memasukkan password salah | Pesan "Username atau password salah."; tetap di halaman Masuk | Masuk ke Dashboard / pesan lain | Sesuai ( ) Tidak Sesuai ( ) |
| 4 | Masuk | Memasukkan username yang tidak terdaftar | Pesan sama persis dengan No. 3 | Pesan berbeda yang membocorkan username tidak ada | Sesuai ( ) Tidak Sesuai ( ) |
| 5 | Masuk | Mengosongkan username/password lalu tekan Masuk | Pesan "Username dan password wajib diisi." | Aplikasi error / mencoba masuk | Sesuai ( ) Tidak Sesuai ( ) |
| 6 | Masuk | Masuk dengan akun demo lama `admin` / `pyrosnap` | Ditolak dengan pesan No. 3 | Berhasil masuk | Sesuai ( ) Tidak Sesuai ( ) |
| 7 | Masuk | Mematikan PostgreSQL lalu mencoba masuk | Pesan "Database tidak tersedia, login tidak dapat diperiksa …" | Pesan "password salah" / aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |
| 8 | Daftar Akun Warga | Mengisi username, password (≥ 8), dan ulangi password dengan benar | Akun dibuat; langsung masuk sebagai Publik; panel "Wilayah Saya" tampil | Akun tidak dibuat / tidak masuk | Sesuai ( ) Tidak Sesuai ( ) |
| 9 | Daftar Akun Warga | Mengisi username yang sudah dipakai | Pesan "Username sudah dipakai…"; akun tidak dibuat | Akun ganda dibuat / aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |
| 10 | Daftar Akun Warga | Password < 8 karakter atau ulangi password berbeda | Pesan kesalahan sesuai; akun tidak dibuat | Akun tetap dibuat | Sesuai ( ) Tidak Sesuai ( ) |
| 11 | Masuk sebagai Tamu | Menekan "Masuk sebagai Tamu" | Dashboard tampil; profil "Tamu"; menu Ekspor dan tombol Ekspor tidak ada | Tidak masuk / tombol Ekspor tampil | Sesuai ( ) Tidak Sesuai ( ) |
| 12 | Panduan & Kontak Darurat | Menekan tombol Panduan di halaman Masuk (tanpa login) | Halaman Panduan tampil (nomor darurat, saat ada asap, sumber); Kembali → halaman Masuk | Halaman tidak terbuka / diminta login | Sesuai ( ) Tidak Sesuai ( ) |
| 13 | Keluar | Menekan "Keluar", lalu menekan Alt + ← | Kembali ke halaman Masuk; Alt + ← tidak membuka Dashboard | Dashboard terbuka lagi tanpa login | Sesuai ( ) Tidak Sesuai ( ) |
| 14 | Masuk | Hanya memakai keyboard: Tab antar-isian, Enter untuk masuk | Cincin fokus terlihat di setiap isian/tombol; Enter mengirim form | Fokus tidak terlihat / Enter tidak berfungsi | Sesuai ( ) Tidak Sesuai ( ) |

## B. Halaman Admin

| No. | Halaman yang Diuji | Aksi Aktor | Reaksi Sistem — Benar | Reaksi Sistem — Salah | Hasil |
|---|---|---|---|---|---|
| 1 | Dashboard | Membuka Dashboard setelah masuk | Data dari NASA FIRMS dimuat; bilah status menyebut jumlah titik dan titik baru disimpan ke database | Data kosong tanpa penjelasan / aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |
| 2 | Dashboard | Menekan "Perbarui dari NASA FIRMS" untuk kedua kalinya | Status "0 titik baru …" dan "peringatan sudah pernah dikirim" | Data/peringatan tergandakan | Sesuai ( ) Tidak Sesuai ( ) |
| 3 | Dashboard | Memutus internet lalu memperbarui data | Pesan gagal memuat dari NASA FIRMS; data terakhir dari database tetap tampil | Aplikasi error / daftar kosong tanpa pesan | Sesuai ( ) Tidak Sesuai ( ) |
| 4 | Cari Lokasi | Memilih/mengetik "Riau" lalu Cari | Peta fokus ke Riau; daftar hanya titik di sekitar Riau | Peta/daftar tidak berubah | Sesuai ( ) Tidak Sesuai ( ) |
| 5 | Cari Lokasi | Mengetik lokasi yang tidak ada (mis. "Bandung") | Pesan berisi daftar area pantau yang tersedia | Aplikasi error / tidak ada pesan | Sesuai ( ) Tidak Sesuai ( ) |
| 6 | Filter | Keyakinan minimum 80 → Terapkan Filter; lalu Reset | Hanya titik tingkat Tinggi; Reset mengembalikan semua titik | Titik tingkat lain masih tampil / Reset tidak berfungsi | Sesuai ( ) Tidak Sesuai ( ) |
| 7 | Peta & Daftar | Memilih satu baris di daftar | Titik yang sama tersorot (tepi hitam) di peta | Tidak ada sorotan / titik lain tersorot | Sesuai ( ) Tidak Sesuai ( ) |
| 8 | Peta & Legenda | Mengamati peta dan legenda | Tiga tingkat dibedakan warna, ukuran, dan tepi (Tinggi: besar, tepi gelap tebal); legenda "Rendah (< 30%) / Nominal (30–79%) / Tinggi (≥ 80%)" | Tingkat hanya beda warna / label tidak sesuai | Sesuai ( ) Tidak Sesuai ( ) |
| 9 | Detail | Klik dua kali baris (atau Enter) | Detail tampil: keyakinan deteksi + kelas, wilayah, koordinat, waktu lokal & UTC, FRP, kecerahan | Detail tidak terbuka / data kosong | Sesuai ( ) Tidak Sesuai ( ) |
| 10 | Detail | Menekan Kembali setelah filter diterapkan | Kembali ke Dashboard dengan filter & daftar yang sama, tanpa memuat ulang | Filter hilang / data dimuat ulang | Sesuai ( ) Tidak Sesuai ( ) |
| 11 | Ringkasan per Wilayah | Membuka tab "Ringkasan per Wilayah" | Tabel jumlah titik per area (Tinggi/Nominal/Rendah, FRP maks) | Tab kosong padahal ada data | Sesuai ( ) Tidak Sesuai ( ) |
| 12 | Data Historis | Memilih rentang ≤ 31 hari → "Tampilkan rentang"; lalu "Kembali ke data terbaru" | Titik pada rentang itu tampil; tombol kembali memulihkan data terbaru | Data tidak berubah / aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |
| 13 | Data Historis | Memilih rentang > 31 hari atau tanggal akhir sebelum awal | Pesan kesalahan rentang | Query tetap dijalankan / aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |
| 14 | Ekspor Laporan | Memilih CSV → Ekspor ke File → simpan | File tersimpan; pesan "Tercatat di database sebagai laporan #…"; dibuka di Excel: 6 kolom, desimal titik | File tidak tersimpan / kolom pecah | Sesuai ( ) Tidak Sesuai ( ) |
| 15 | Ekspor Laporan | Mengekspor ke file yang sedang dibuka di Excel | Pesan "Laporan gagal disimpan …"; aplikasi tetap jalan | Aplikasi error / tertutup | Sesuai ( ) Tidak Sesuai ( ) |
| 16 | Peringatan | Menekan lonceng di bilah atas (atau menu Peringatan) → pilih → "Tandai dibaca" | Daftar peringatan tampil; angka di lencana lonceng dan kartu "Belum dibaca" berkurang | Daftar kosong / angka tidak berubah | Sesuai ( ) Tidak Sesuai ( ) |
| 17 | Peringatan | Klik dua kali sebuah peringatan → Kembali | Detail titik terkait terbuka; Kembali ke halaman Peringatan | Detail titik lain / kembali ke tempat salah | Sesuai ( ) Tidak Sesuai ( ) |

## C. Halaman Operator (posko & petugas lapangan)

| No. | Halaman yang Diuji | Aksi Aktor | Reaksi Sistem — Benar | Reaksi Sistem — Salah | Hasil |
|---|---|---|---|---|---|
| 1 | Dashboard | Masuk sebagai `operator_uji` | Data dimuat dari database; tombol "Perbarui dari NASA FIRMS" tidak ada; lonceng dan menu Peringatan ada | Tombol Perbarui tampil / data tidak dimuat | Sesuai ( ) Tidak Sesuai ( ) |
| 2 | Verifikasi Lapangan (Detail) | Memilih status "Api aktif", mengisi catatan, Simpan Verifikasi | Pesan tersimpan; "Status terakhir: Api aktif …" dan riwayat menampilkan nama pemeriksa | Tidak tersimpan / status tidak berubah | Sesuai ( ) Tidak Sesuai ( ) |
| 3 | Verifikasi Lapangan (Detail) | Menekan Simpan tanpa memilih status | Pesan "Pilih status hasil cek lapangan dulu." | Data tersimpan tanpa status / aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |
| 4 | Navigasi (Detail) | Menekan "Salin Koordinat", lalu tempel di aplikasi lain | Pesan koordinat disalin; hasil tempel sama dengan koordinat di Detail | Tidak ada yang tersalin | Sesuai ( ) Tidak Sesuai ( ) |
| 5 | Navigasi (Detail) | Menekan "Buka Arah di Peta Daring" | Browser terbuka menampilkan arah ke titik | Browser tidak terbuka dan tidak ada pesan | Sesuai ( ) Tidak Sesuai ( ) |
| 6 | Peringatan | Membuka menu Peringatan | Peringatan yang sama dengan Admin tampil | Operator tidak menerima peringatan | Sesuai ( ) Tidak Sesuai ( ) |
| 7 | Ekspor Laporan | Mengekspor laporan TXT | File tersimpan dan tercatat di database | Tombol Ekspor tidak ada / gagal | Sesuai ( ) Tidak Sesuai ( ) |

## D. Halaman Publik (warga dengan akun)

| No. | Halaman yang Diuji | Aksi Aktor | Reaksi Sistem — Benar | Reaksi Sistem — Salah | Hasil |
|---|---|---|---|---|---|
| 1 | Dashboard | Masuk dengan akun Publik | Panel "Wilayah Saya" tampil; tombol Perbarui, lonceng/menu Peringatan, dan Data historis tidak ada | Fitur petugas tampil | Sesuai ( ) Tidak Sesuai ( ) |
| 2 | Wilayah Saya | Memilih wilayah → Simpan; keluar lalu masuk lagi | Banner peringatan sesuai wilayah; pilihan masih tersimpan setelah masuk lagi | Pilihan hilang / banner tidak berubah | Sesuai ( ) Tidak Sesuai ( ) |
| 3 | Detail | Membuka Detail titik yang sudah diverifikasi | Status verifikasi tampil (tanpa nama pemeriksa); form verifikasi & tombol navigasi tidak ada | Form verifikasi/navigasi tampil | Sesuai ( ) Tidak Sesuai ( ) |
| 4 | Ekspor Laporan | Mengekspor laporan CSV | File tersimpan (Publik boleh ekspor) | Tombol Ekspor tidak ada / ditolak | Sesuai ( ) Tidak Sesuai ( ) |

## E. Halaman Tamu (warga tanpa akun)

| No. | Halaman yang Diuji | Aksi Aktor | Reaksi Sistem — Benar | Reaksi Sistem — Salah | Hasil |
|---|---|---|---|---|---|
| 1 | Wilayah Saya | Memilih wilayah → Simpan; Keluar lalu masuk lagi sebagai Tamu | Banner sesuai wilayah; setelah keluar, pilihan hilang ("hanya untuk sesi") | Pilihan tetap tersimpan | Sesuai ( ) Tidak Sesuai ( ) |
| 2 | Dashboard | Mencari tombol/menu Ekspor dan lonceng/menu Peringatan | Keduanya tidak ada | Salah satu tampil | Sesuai ( ) Tidak Sesuai ( ) |
| 3 | Dashboard | Database dimatikan, lalu masuk sebagai Tamu | Pesan "Data titik panas belum bisa dimuat …"; tombol Panduan tetap bisa dibuka | Aplikasi error / tertutup | Sesuai ( ) Tidak Sesuai ( ) |

## F. Layanan Web API Pihak Ketiga (Open-Meteo, OpenStreetMap Nominatim; `docs/backend/web-api.md`)

| No. | Halaman yang Diuji | Aksi Aktor | Reaksi Sistem — Benar | Reaksi Sistem — Salah | Hasil |
|---|---|---|---|---|---|
| 1 | Detail — Kondisi sekitar | Membuka Detail sebuah titik | Kartu "Kondisi sekitar" menampilkan wilayah administratif (desa/kelurahan, kecamatan, kota/kabupaten, provinsi) dengan label "perkiraan" | Nama wilayah kosong tanpa keterangan | Sesuai ( ) Tidak Sesuai ( ) |
| 2 | Detail — Kondisi sekitar | Membuka Detail sebuah titik | Kualitas udara tampil: PM2.5 (µg/m³), chip kategori + AQI (mis. "Baik · AQI 41"), saran singkat; atribusi "Weather data by Open-Meteo.com" ada dan bisa diklik | Data tidak tampil / tanpa atribusi | Sesuai ( ) Tidak Sesuai ( ) |
| 3 | Detail — Kondisi sekitar | Membuka Detail sebuah titik | Kecepatan angin (km/jam), arah asal angin + perkiraan arah asap (mis. "dari Barat Daya … asap ke Timur Laut"), kelembapan, suhu, hujan, dan jam data tampil | Data tidak tampil | Sesuai ( ) Tidak Sesuai ( ) |
| 4 | Detail — Kondisi sekitar | Memutus internet lalu membuka Detail titik yang belum pernah dibuka | Dalam ±10 detik tertulis "Tidak tersedia" + "… tidak bisa dihubungi. Periksa koneksi internet."; data titik dan verifikasi tetap tampil | Detail gagal dibuka / aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |
| 5 | Wilayah saya (Publik/Tamu) | Memilih wilayah lalu Simpan | Kotak "Kualitas udara di <wilayah>" tampil dengan PM2.5, kategori bahasa awam, saran, dan atribusi Open-Meteo | Kualitas udara tidak tampil | Sesuai ( ) Tidak Sesuai ( ) |
| 6 | Detail | Membuka beberapa Detail berturut-turut dengan cepat | Nama wilayah tetap muncul (permintaan diantre ≥ 1 detik); titik yang sama tidak diminta ulang | Aplikasi tertahan / diblokir penyedia (HTTP 403/429) | Sesuai ( ) Tidak Sesuai ( ) |
| 7 | Detail — Kondisi sekitar | Setelah No. 4, menyambung internet lalu menekan tombol muat ulang (ikon panah melingkar) di kartu | Data tambahan termuat tanpa membuka ulang halaman | Tetap "tidak tersedia" / aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |

## G. Instalasi & Konfigurasi ⏳ (setelah pengemasan MVP-19…26)

| No. | Halaman yang Diuji | Aksi Aktor | Reaksi Sistem — Benar | Reaksi Sistem — Salah | Hasil |
|---|---|---|---|---|---|
| 1 | Instalasi | Memasang dari paket di komputer Windows bersih (tanpa SDK) | Aplikasi terpasang dan halaman Masuk terbuka | Gagal pasang / butuh SDK | Sesuai ( ) Tidak Sesuai ( ) |
| 2 | Konfigurasi basis data | Menjalankan skrip database untuk Windows sesuai panduan | Database dan 9 tabel terbentuk; seed sumber data ada | Skrip gagal / tabel kurang | Sesuai ( ) Tidak Sesuai ( ) |
| 3 | Konfigurasi aplikasi | Menjalankan aplikasi tanpa `FIRMS_MAP_KEY` | Pesan jelas "Key NASA FIRMS belum diatur …" | Aplikasi error / tanpa pesan | Sesuai ( ) Tidak Sesuai ( ) |
| 4 | Konfigurasi aplikasi | Menjalankan aplikasi tanpa WebView2 Runtime | Peta sederhana (kanvas) tampil sebagai cadangan, atau pemasang meminta WebView2 | Aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |

## H. Tampilan & Tema (DS-10)

Jalankan dengan satu akun Admin dan satu akun Tamu. Kasus 1–5 diulang **di tema terang dan tema gelap**.

| No. | Halaman yang Diuji | Aksi Aktor | Reaksi Sistem — Benar | Reaksi Sistem — Salah | Hasil |
|---|---|---|---|---|---|
| 1 | Semua halaman | Membuka Masuk, Daftar, Dashboard, Detail, Peringatan, Ekspor, Panduan | Kartu, teks, tombol, input, tabel, tab, slider, dan scrollbar terbaca jelas; tidak ada kotak putih/hitam yang salah warna (kecuali kalender DatePicker, batasan tercatat) | Teks tidak terbaca / kontrol bawaan berwarna sistem | Sesuai ( ) Tidak Sesuai ( ) |
| 2 | Menu samping & bilah atas | Berpindah antar-menu | Menu aktif bergaris kiri dan tebal; judul bilah atas sesuai halaman; ikon tampil (bukan kotak kosong) | Ikon kotak / menu aktif tidak terlihat | Sesuai ( ) Tidak Sesuai ( ) |
| 3 | Dashboard | Mengubah filter dan cari lokasi | Empat kartu KPI ikut berubah sesuai titik yang tampil | Angka KPI tidak berubah / berbeda dengan daftar | Sesuai ( ) Tidak Sesuai ( ) |
| 4 | Peta | Membuka Dashboard dengan WebView2 | Peta mengikuti tema (tile gelap di tema gelap), legenda terbaca, titik terpilih tersorot | Peta tetap terang / legenda tak terbaca | Sesuai ( ) Tidak Sesuai ( ) |
| 5 | Semua halaman | Hanya memakai keyboard (Tab, Enter, Esc) | Cincin fokus biru terlihat di tombol, menu, input, slider, segmen | Fokus tidak terlihat | Sesuai ( ) Tidak Sesuai ( ) |
| 6 | Ganti tema | Di Dashboard setelah memfilter, tekan "Tema gelap" di menu samping | Tema berganti; filter, pilihan titik, dan data tetap; tombol berubah jadi "Tema terang" | Data/filter hilang / aplikasi error | Sesuai ( ) Tidak Sesuai ( ) |
| 7 | Ganti tema | Tutup aplikasi lalu buka lagi | Tema terakhir dipakai sejak halaman Masuk | Kembali ke tema terang | Sesuai ( ) Tidak Sesuai ( ) |
| 8 | Masuk | Menekan tombol tema di pojok kanan atas halaman Masuk | Tema berganti tanpa perlu masuk | Tombol tidak berfungsi | Sesuai ( ) Tidak Sesuai ( ) |

## Rekap hasil

| Bagian | Jumlah kasus | Sesuai | Tidak Sesuai | Catatan |
|---|---|---|---|---|
| A. Umum | 14 | | | |
| B. Admin | 17 | | | |
| C. Operator | 7 | | | |
| D. Publik | 4 | | | |
| E. Tamu | 3 | | | |
| F. Web API | 7 | | | |
| G. Instalasi ⏳ | 4 | | | |
| H. Tampilan & Tema | 8 (×2 tema untuk No. 1–5) | | | |
| **Total** | **64** | | | |

Kasus "Tidak Sesuai" dicatat di `docs/debug/bug-fix.md`, diperbaiki, lalu diuji ulang (siklus Membandingkan → perbaikan → uji ulang pada Modul 10; sanity testing).

## Pemetaan ke skenario lama
A ↔ ACC-01…ACC-17 (`database/acount-testing.md`); B/C ↔ TST-04, TST-10…TST-16, TST-34…TST-36 dan OPS/PUB (`testing-schema.md`, `progress/backend-progress.md`); D/E ↔ ACC-12…ACC-17, FLOW-01/02; F ↔ WEB-09 (`backend/web-api.md`); G ↔ MVP-19…26 (`MVP.md`).
