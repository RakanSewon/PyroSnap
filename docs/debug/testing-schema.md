# Skema Pengujian PyroSnap

> Status: **rencana**. Belum ada tes yang ditulis atau dijalankan. Disusun 2026-11-22.

## 1. Tujuan

Menetapkan cara membuktikan bahwa tiap bug di `bug-fix.md` benar-benar selesai dan tiap fitur di `feature/main-feature.md` berfungsi, tanpa menambah infrastruktur yang belum disetujui.

## 2. Pendekatan & batasan

- **Fase A (sekarang):** uji manual terstruktur + skrip SQL + data CSV contoh. Tidak ada proyek unit test baru (akan jadi **ARCH-B4**, menunggu persetujuan).
- **UI WPF hanya dapat diverifikasi di Windows** (dengan WebView2 Runtime). Lingkungan Linux hanya cukup untuk uji SQL dan membaca kode.
- Setiap uji punya ID, langkah, hasil yang diharapkan, dan status. Hasil dicatat di tabel bagian 8.
- Data uji **tidak boleh berisi key asli atau password nyata**.

## 3. Data uji

Berkas (disimpan di `docs/debug/samples/` saat dibuat; belum ada):

| Berkas | Isi |
|---|---|
| `firms-valid.csv` | Header + beberapa baris asli (diambil dari FIRMS, 1 baris dulu untuk konfirmasi kolom) |
| `firms-bad-row.csv` | Satu baris dengan angka rusak di antara baris valid |
| `firms-short-row.csv` | Baris dengan kolom kurang |
| `firms-empty.csv` | Hanya header |

## 4. Uji parsing & sumber data (F1)

| ID | Kasus | Langkah | Hasil diharapkan | Bug |
|---|---|---|---|---|
| TST-01 | Skema kolom dikonfirmasi | Ambil 1 baris CSV asli, cocokkan header | Daftar kolom + indeks tercatat di `backend/data-source-api.md` | BUG-03 |
| TST-02 | Confidence terbaca | Parse baris dengan confidence `h`, `n`, `l` | Terpetakan sesuai keputusan SCH-04, bukan 50 | BUG-03 |
| TST-03 | Waktu akuisisi | Parse baris dengan `acq_date`/`acq_time` tertentu | `WaktuAkuisisi` = tanggal+jam CSV (UTC) | BUG-04 |
| TST-04 | Gagal jaringan | Matikan internet, tekan Muat Ulang | Pesan error jelas; tidak ada titik dummy tanpa label | BUG-05 |
| TST-05 | Baris rusak | Parse `firms-bad-row.csv` | Baris valid tetap terbaca; jumlah dilewati = 1 | BUG-06 |
| TST-06 | Kolom kurang / file kosong | Parse `firms-short-row.csv`, `firms-empty.csv` | Tidak crash; hasil kosong dengan pesan | BUG-06 |
| TST-07 | Timeout | Blokir host FIRMS (atau jaringan lambat) | Gagal ≤ timeout yang ditetapkan, tombol aktif lagi | BUG-15 |
| TST-08 | Key tak bocor | `git grep` pola key + periksa `.env` di-gitignore | Nol temuan di working tree | BUG-01, BUG-02 |

## 5. Uji filter, navigasi, export (F1)

| ID | Kasus | Langkah | Hasil diharapkan | Bug |
|---|---|---|---|---|
| TST-10 | Filter komposable | Cari "Riau", lalu confidence min tinggi; ulangi urutan terbalik | Dua urutan → daftar sama | BUG-13 |
| TST-11 | Reset | Terapkan beberapa filter lalu Reset | Jumlah titik kembali ke total | FT-02b |
| TST-12 | State bertahan | Filter → Detail → Kembali | Filter & daftar sama; tak ada request baru (cek di log/proxy) | BUG-08 |
| TST-13 | Peta tidak melompat | Terapkan filter berulang | Zoom/posisi peta tak berubah paksa | BUG-14 |
| TST-14 | CSV locale | Ubah region Windows ke Indonesia, export CSV, buka di Excel | 6 kolom, desimal titik | BUG-07 |
| TST-15 | Export file terkunci | Buka file target di Excel lalu export ke file yang sama | Pesan error, aplikasi tidak crash | BUG-16 |
| TST-16 | Sort kolom | Klik header FRP/Confidence | Terurut numerik | BUG-09 |

## 6. Uji database (F2, F4) — dapat dijalankan di Linux dengan `psql`

| ID | Kasus | Langkah | Hasil diharapkan | Ref |
|---|---|---|---|---|
| TST-20 | Skema dari nol | DB baru → jalankan skrip skema + migrasi | Tanpa error; 8 tabel | DBA-03 |
| TST-21 | Constraint | Insert hotspot `confidence=150`, `latitude=200` | Ditolak oleh CHECK | skema |
| TST-22 | FK RESTRICT | Hapus `hotspot_data_source` yang dipakai hotspot | Ditolak | skema |
| TST-23 | Upsert tidak menggandakan | Sync/insert set data yang sama dua kali | Jumlah baris `hotspot` tidak bertambah | DBA-04, SCH-01 |
| TST-24 | Format laporan TXT | Insert `hotspot_report` dengan format `TXT` | Diterima setelah SCH-03 | BUG-11 |
| TST-25 | Transaksi laporan | Simulasi gagal di tengah `SaveAsync` | Tidak ada `hotspot_report` yatim / `report_hotspot` sebagian | DBA-07 |
| TST-26 | Cascade | Hapus user uji | `alert`/`filter_criteria` miliknya ikut terhapus; `hotspot_report` menolak (RESTRICT) | skema |
| TST-27 | Koneksi mati | Hentikan PostgreSQL, buka aplikasi | Aplikasi terbuka dengan indikator DB tidak tersedia | DBA-02 |
| TST-28 | Konfigurasi | Hapus env, isi `.env` | Koneksi memakai `.env` | DBA-01, BUG-12 |

## 7. Uji login, peran, alert (F3, F5)

Skenario akun ada di `docs/database/acount-testing.md` (ACC-xx).

| ID | Kasus | Hasil diharapkan | Ref |
|---|---|---|---|
| TST-30 | Login benar per peran | Masuk; menu sesuai peran | ACC-02 |
| TST-31 | Login salah | Pesan generik; tidak ada petunjuk username ada/tidak | ACC-03 |
| TST-32 | Kredensial hardcoded hilang | Akun demo lama tidak lagi bisa login | BUG-10 |
| TST-33 | Logout | Sesi, pilihan, filter dibersihkan | FT-06c |
| TST-34 | Alert terpicu | Data uji di atas ambang → alert dibuat sekali | FT-07 |
| TST-35 | Alert tidak terpicu | Data di bawah ambang → tidak ada alert | FT-07 |
| TST-36 | Alert tersimpan & dibaca | Tersimpan di tabel; `MarkAsRead` mengubah `is_read` | FT-07c/d |

## 8. Catatan hasil (diisi saat uji dijalankan)

| ID | Tanggal | Penguji | Lingkungan | Hasil (lulus/gagal) | Catatan |
|---|---|---|---|---|---|
| TST-01 | 2026-11-22 | Claude | Linux, data FIRMS asli | Lulus | Header 14 kolom; confidence indeks 9 `l/n/h`; `acq_time` tanpa nol di depan; SNPP kosong sejak 2026-11-02 |
| TST-02, TST-03 | 2026-11-22 | Claude | Linux, program uji sementara | Lulus | 2.261 baris NOAA-20: 134/2.108/19 → 30/60/90; waktu UTC benar |
| TST-04 | 2026-11-22 | Claude | Linux (logika) | Lulus | `SyncAsync` ke host mati = false + pesan; tampilan UI belum dicek |
| TST-05, TST-06 | 2026-11-22 | Claude | Linux (logika) | Lulus | Baris rusak/pendek dilewati (3); hanya header = 0 titik; respons kosong / bukan CSV = pesan jelas |
| TST-08 | 2026-11-22 | Claude | Linux | Lulus | Key lama tidak ada di working tree (masih di riwayat git) |
| TST-14 | 2026-11-22 | Claude | Linux, culture id-ID | Lulus (logika) | 6 kolom, desimal titik; belum dibuka di Excel |
| TST-15 | 2026-11-22 | Claude | Linux (model) | Lulus (model) | `ExportToFile` melempar IOException; dialog UI belum dicek |
| Live | 2026-11-22 | Claude | Linux, API FIRMS | Lulus | NOAA-20 1 hari: 451 titik; key salah → "Permintaan ditolak (400)" |
| TST-20…TST-24, TST-26 | 2026-11-22 | Claude | PostgreSQL 18.6 lokal, tabel sementara (`search_path = pg_temp`) | Lulus | `001`–`007` dari nol; upsert tidak ganda; CHECK/RESTRICT/CASCADE benar; TXT diterima; alert unik; waktu UTC; seed idempoten. DB asli tidak disentuh |
| TST-27, TST-28 | 2026-11-22 | Claude | Linux, program uji sementara | Lulus (logika) | Koneksi benar via `.env`; password salah, port mati, dan konfigurasi kosong memberi pesan jelas. Tampilan di halaman Login belum dicek di Windows |
| TST-30…TST-33 (logika) | 2026-11-22 | Claude | Linux, program uji sementara + DB lokal (hanya SELECT) | Lulus | Hash benar/salah, username beda, logout, hash rusak/kosong tidak crash, hash `$2a$` (format pgcrypto) terverifikasi, username tak dikenal → null, input injeksi aman, DB mati → NpgsqlException. Login nyata menunggu akun uji |
| Seed guard | 2026-11-22 | Claude | DB lokal, tabel sementara + ROLLBACK | Lulus | Penanda `<password-...>` belum diganti → seed dibatalkan; sudah diganti → lolos. Tabel `users` asli tetap 0 baris |
| TST-23, TST-25, F4 | 2026-11-22 | Claude | DB lokal, tabel sementara (`pg_temp`), 2.261 baris FIRMS asli | Lulus (26/26) | Sync ulang 0 baru; muat 24 jam = 597 titik identik dengan `IsMatch`; 4 kombinasi filter DB = memori; laporan 100 hotspot tertaut; FK gagal → rollback total (laporan & hotspot); hapus laporan tidak menghapus hotspot. Tabel asli tidak tersentuh |
| F7 backend | 2026-11-22 | Claude | DB lokal, tabel sementara, data FIRMS asli | Lulus (37/37) | Migrasi 008 (Analis → Admin, Analis ditolak), 009; daftar Publik (validasi, hash, username ganda); alert hanya Admin & Operator; peringatan membawa hotspot; verifikasi (riwayat, status asing/catatan panjang ditolak, akun dihapus → pemeriksa null); wilayah pilihan; area pantau. Tabel asli tidak tersentuh |
| FE matriks peran | 2026-11-22 | Claude | Linux, program uji `AppSession` | Lulus (7/7) | Admin, Analis lama (= Admin), Operator, Publik, Tamu, tanpa sesi, user logout — sesuai `role-based-ui.md` §2 |
| FE peta (`map.html`) | 2026-11-22 | Claude | Node 24 + tiruan Leaflet | Lulus (10/10) | Legenda dari konfigurasi LAPAN, kelas 20/30/79/80, warna & garis tepi Tinggi, tooltip istilah baru, fit hanya saat data baru, sorot & lepas sorot |
| Parser (ulang) | 2026-11-22 | Claude | Linux, data FIRMS asli | Lulus (30/30) | Pemetaan `l=20` (kelas Rendah) |
| TST-34…TST-36, F5 | 2026-11-22 | Claude | DB lokal, tabel sementara (`pg_temp`), data FIRMS asli | Lulus (23/23) | 19 hotspot kritis × 3 user = 57 alert; sinkron ulang 0 baru; conf 60 tidak memicu, conf 90 memicu walau FRP kecil; batas 50 per sinkron; tandai dibaca hanya milik sendiri; CASCADE saat user dihapus. Tabel asli tidak tersentuh |

## 9. Checklist target

- [ ] **TST-A1** — Berkas data uji CSV dibuat (tanpa key). *Selesai bila* 4 berkas di `docs/debug/samples/`.
- [ ] **TST-A2** — Semua uji P0 (TST-01…TST-08) dijalankan dan lulus.
- [ ] **TST-A3** — Semua uji F1 (TST-10…TST-16) dijalankan di Windows dan lulus.
- [ ] **TST-A4** — Uji DB (TST-20…TST-28) dijalankan pada DB baru dan lulus.
- [ ] **TST-A5** — Uji login/alert (TST-30…TST-36) lulus.
- [ ] **TST-A6** — Tiap bug yang dipindah ke "Bug yang Diselesasikan" menyebut ID uji yang lulus.
- [ ] **TST-B1** — (Opsional, menunggu ARCH-B4) otomatisasi uji parser & filter sebagai unit test.

## 10. Rujukan
`docs/debug/bug-fix.md`, `docs/feature/main-feature.md`, `docs/database/acount-testing.md`, `Claude.md` §8
