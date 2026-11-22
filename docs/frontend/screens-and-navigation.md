# Layar & Navigasi PyroSnap

> Status: **rencana**. Disusun 2026-11-22. Tema visual dipertahankan dulu; sistem desain baru dibahas terpisah (`docs/ui/design-system.md`).
> Hak akses per peran: `docs/frontend/role-based-ui.md`. Masalah UX per layar: `docs/ui/ux/ux-audit.md`.

## 1. Tujuan

Mendata semua layar (yang sudah ada dan yang akan dibuat), alur perpindahannya, dan aturan penjaga (siapa boleh membuka apa), supaya pekerjaan frontend Fase 7 punya peta yang jelas.

## 2. Inventaris layar

### 2.1 Sudah ada

| ID | Layar | File | Isi | Catatan |
|---|---|---|---|---|
| SCR-01 | Login | `Views/LoginView.xaml` | Username, password, status database | Perlu jalur **Daftar** dan **Masuk sebagai tamu** |
| SCR-02 | Dashboard | `Views/DashboardView.xaml` | Cari lokasi, filter, peta (Leaflet/WebView2 + fallback kanvas), daftar hotspot, status sinkron, tombol 🔔 Alert | Kode 675 baris; perlu dipecah (FE-02) |
| SCR-03 | Detail hotspot | `Views/DetailView.xaml` | Koordinat, confidence, brightness, FRP, waktu | Perlu verifikasi lapangan, navigasi, status verifikasi |
| SCR-04 | Export laporan | `Views/ExportView.xaml` | Pilih TXT/CSV, preview, simpan + catat ke DB | Tidak boleh untuk Tamu |
| SCR-05 | Alert | `Views/AlertView.xaml` | Daftar alert, tandai dibaca | Khusus Admin & Operator setelah F7 |

### 2.2 Baru (Fase 7)

| ID | Layar / bagian | Bentuk usulan | Untuk | Rujukan |
|---|---|---|---|---|
| SCR-06 | Daftar akun | Halaman baru `RegisterView` dari tombol di Login | Warga (akun Publik) | K10, ROLE-04 |
| SCR-07 | Masuk sebagai tamu | Tombol di Login → Dashboard mode Tamu | Warga tanpa akun | K10 |
| SCR-08 | Wilayah pilihan + peringatan warga | Panel di Dashboard (pilih wilayah) + banner peringatan berbahasa awam | Publik (tersimpan), Tamu (hanya sesi) | PUB-01 |
| SCR-09 | Ringkasan risiko per wilayah | Kartu/tabel di Dashboard (jumlah & tingkat per wilayah hari ini) | Semua | PUB-02 |
| SCR-10 | Panduan & kontak darurat | Halaman baru `PanduanView` | Semua (tombol jelas untuk Publik/Tamu) | PUB-03 |
| SCR-11 | Verifikasi lapangan | Bagian di Detail: status + catatan + riwayat singkat | Isi: Admin & Operator; lihat: semua | OPS-01 |
| SCR-12 | Navigasi ke titik | Tombol di Detail: salin koordinat, buka arah | Admin & Operator | OPS-02 |
| SCR-13 | Data historis | Rentang tanggal di panel filter Dashboard | Admin & Operator | OPS-03 |
| SCR-14 | Kelola akun ⏳ | Halaman Admin (belum disetujui sebagai pekerjaan F7) | Admin | matriks §5.2 |
| SCR-15 | Riwayat laporan ⏳ | Halaman Admin (belum disetujui sebagai pekerjaan F7) | Admin | matriks §5.2 |

## 3. Alur navigasi

```
                 ┌──────────── Daftar (SCR-06) ──┐ berhasil → login otomatis (Publik)
                 │                                ▼
Login (SCR-01) ──┼── Login ─────────────────▶ Dashboard (SCR-02) ──┬─▶ Detail (SCR-03) ──▶ kembali
                 │                                ▲                ├─▶ Export (SCR-04)*   ──▶ kembali
                 └── Masuk sebagai tamu ──────────┘                ├─▶ Alert (SCR-05)*    ──▶ kembali
                                                                   ├─▶ Panduan (SCR-10)   ──▶ kembali
                                                                   └─▶ Logout / Keluar ──▶ Login
* sesuai peran (role-based-ui.md)
```

Aturan:
1. Semua halaman selain Login, Daftar, dan Panduan wajib punya sesi (user login atau Tamu). Tanpa sesi → kembali ke Login.
2. Halaman yang dibatasi peran (Export untuk Tamu, Alert untuk Publik/Tamu) dijaga di **dua tempat**: tombolnya disembunyikan, dan halamannya menolak dibuka (penjaga di `Loaded`).
3. Riwayat navigasi `Frame` dihapus setiap pindah halaman (sudah ada sejak F3), jadi Back/Alt+← tidak membuka halaman lama.
4. Kembali ke Dashboard memakai state di `AppSession` (data, filter, pilihan) tanpa memuat ulang (sudah ada sejak F1).
5. Panduan & kontak darurat sebaiknya bisa dibuka **tanpa login** (dari Login) karena informasinya penting untuk siapa pun.

## 4. Checklist target

- [x] **SCR-A1** — Navigasi terpusat (FE-03): satu helper untuk pindah halaman + penjaga sesi/peran, menggantikan cast `((MainWindow)Window.GetWindow(this)).Navigate(...)` yang tersebar.
- [x] **SCR-A2** — Layar Daftar (SCR-06): username, password, konfirmasi; validasi di tempat; akun baru berperan Publik; setelah berhasil langsung masuk.
- [x] **SCR-A3** — Mode Tamu (SCR-07): masuk tanpa akun, data dari database, label "Tamu" di header, tanpa Export/Alert.
- [x] **SCR-A4** — Panel wilayah pilihan + banner peringatan warga (SCR-08).
- [x] **SCR-A5** — Ringkasan risiko per wilayah (SCR-09). Butuh keputusan K2 (atribut wilayah).
- [x] **SCR-A6** — Halaman Panduan (SCR-10), bisa dibuka dari Login dan Dashboard.
- [x] **SCR-A7** — Detail: verifikasi lapangan (SCR-11) + navigasi (SCR-12) sesuai peran.
- [x] **SCR-A8** — Rentang tanggal untuk data historis (SCR-13) sesuai peran.
- [x] **SCR-A9** — Penjaga peran di setiap halaman terbatas (aturan §3.2).

## 5. Keputusan terbuka
1. *(Diterapkan 2026-11-22, FK4)* Ringkasan risiko = tab "Ringkasan per Wilayah" di samping daftar; wilayah pilihan = panel "Wilayah Saya" di Dashboard.
2. *(Diterapkan 2026-11-22, FK5)* Panduan bisa dibuka dari halaman Masuk tanpa login.
3. SCR-14/SCR-15 (Kelola akun, Riwayat laporan) masuk F7 atau ditunda?

## 6. Rujukan
`docs/frontend/role-based-ui.md`, `docs/frontend/code-structure.md`, `docs/ui/ux/user-flows.md`, `docs/progress/frontend-progress.md`, `docs/progress/backend-progress.md` (F7)
