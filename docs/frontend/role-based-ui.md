# Tampilan per Peran

> Status: **rencana** (keputusan K9 & K10, 2026-11-22). Kode saat ini masih memakai peran lama Admin/Analis/Operator; perubahan dikerjakan di Fase 7.

## 1. Peran yang dilayani tampilan

| Pengguna | Cara masuk | Peran di DB | Keterangan |
|---|---|---|---|
| **Admin** | Login | `Admin` | Gabungan Admin + Analis lama |
| **Operator** | Login | `Operator` | Posko & petugas lapangan |
| **Publik** | Login setelah **daftar mandiri** | `Publik` | Warga dengan akun |
| **Tamu** | Tombol **Masuk sebagai tamu** | – (tidak ada baris di `users`) | Warga tanpa akun; tidak ada data pribadi yang disimpan |

## 2. Matriks tampilan (✅ tampil/aktif · ❌ disembunyikan · ⏳ fitur belum dibuat)

| Elemen | Admin | Operator | Publik | Tamu |
|---|:-:|:-:|:-:|:-:|
| Peta, daftar, detail hotspot | ✅ | ✅ | ✅ | ✅ |
| Cari lokasi & filter confidence/FRP | ✅ | ✅ | ✅ | ✅ |
| Ringkasan risiko per wilayah ⏳ | ✅ | ✅ | ✅ | ✅ |
| Panduan & kontak darurat ⏳ | ✅ | ✅ | ✅ | ✅ |
| Status verifikasi lapangan (lihat) ⏳ | ✅ | ✅ | ✅ | ✅ |
| Wilayah pilihan + banner peringatan warga ⏳ | ❌ | ❌ | ✅ tersimpan | ✅ hanya sesi |
| Tombol **Muat Ulang Data** (sinkron FIRMS) | ✅ | ❌ (sembunyi) | ❌ | ❌ |
| Tombol **🔔 Alert** (alert operasional) | ✅ | ✅ | ❌ | ❌ |
| Tombol **Export Laporan** | ✅ | ✅ | ✅ (K10) | ❌ |
| Isi verifikasi lapangan di Detail ⏳ | ✅ | ✅ | ❌ | ❌ |
| Navigasi ke titik di Detail ⏳ | ✅ | ✅ | ❌ | ❌ |
| Rentang tanggal (data historis) ⏳ | ✅ | ✅ | ❌ (24 jam) | ❌ (24 jam) |
| Kelola akun / riwayat laporan ⏳ | ✅ | ❌ | ❌ | ❌ |
| Header: nama · peran | nama · Admin | nama · Operator | nama · Publik | "Tamu" |
| Tombol keluar | Logout | Logout | Logout | "Keluar" (ke Login) |

Catatan:
- Operator tetap tidak melihat tombol Muat Ulang. Sejak F3 tombol itu **dinonaktifkan dengan tooltip**; usulan F7 **disembunyikan** supaya layar Operator lebih bersih (keputusan UI, bisa diubah).
- Publik dan Tamu tidak menerima alert operasional "segera penanggulangan"; mereka melihat banner peringatan berbahasa awam (lihat `docs/ui/ux/copy-and-accessibility.md`).
- Data untuk Operator, Publik, dan Tamu dibaca dari database (diisi oleh sinkron Admin).

## 3. Aturan implementasi

1. **Satu sumber hak akses** (ROLE-02 / FE-04): properti di `AppSession`, misalnya `BolehSinkron`, `BolehExport`, `TerimaAlertOperasional`, `BolehVerifikasi`, `BolehNavigasi`, `BolehHistoris`, `IsTamu`. Layar hanya membaca properti ini, tidak membandingkan nama peran sendiri.
2. **Sembunyikan, bukan sekadar nonaktif**, untuk fitur yang memang bukan milik peran itu. Nonaktif + tooltip hanya untuk fitur yang sementara tidak bisa dipakai (mis. saat sedang memuat).
3. **Penjaga ganda**: tombol disembunyikan **dan** halaman/handler memeriksa hak akses lagi (sudah dicontohkan `BtnReload_Click` di F3).
4. Tamu dibuat sebagai sesi khusus di `AppSession` (tanpa `user_id`); fitur yang menulis ke database atas nama user (export tercatat, verifikasi, wilayah tersimpan) tidak tersedia untuknya.

## 4. Checklist target

- [x] **RUI-01** — Properti hak akses lengkap di `AppSession` sesuai §2 (ROLE-02).
- [x] **RUI-02** — Sesi Tamu di `AppSession` + label header "Tamu" + tombol "Keluar".
- [x] **RUI-03** — Dashboard menyembunyikan/menampilkan Muat Ulang, 🔔 Alert, Export, rentang tanggal sesuai peran.
- [x] **RUI-04** — Detail menampilkan verifikasi (isi/lihat) dan navigasi sesuai peran.
- [x] **RUI-05** — Penjaga di Export dan Alert menolak peran yang tidak berhak (aturan §3.3).
- [ ] **RUI-06** — Uji tampilan per peran (Admin, Operator, Publik, Tamu) di Windows, dicatat di `docs/debug/testing-schema.md`.

## 5. Rujukan
`docs/database/acount-testing.md` §5, `docs/feature/additional-feature.md` (ADD-09…13), `docs/progress/backend-progress.md` (F7: ROLE/OPS/PUB)
