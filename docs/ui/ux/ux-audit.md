# Audit UX PyroSnap

> Status: **diperbaiki 2026-11-22 (FE1), belum diuji di Windows**. Audit dari kode dan XAML per 2026-11-22 (belum diamati langsung di Windows). Tema visual dipertahankan; perubahan warna/komponen besar menunggu sistem desain baru (`docs/ui/design-system.md`).
> Prioritas: **P1** = menghambat tugas/menyesatkan · **P2** = mengganggu · **P3** = penyempurnaan.

## 1. Ringkasan temuan

| ID | P | Layar | Temuan | Lokasi |
|---|---|---|---|---|
| UX-01 | P1 | Dashboard | Panel kiri tidak bisa di-scroll dan jendela tanpa ukuran minimum → tombol bawah bisa terpotong di layar kecil | `DashboardView.xaml:52`, `MainWindow.xaml` |
| UX-02 | P2 | Semua | 9 `MessageBox` modal untuk hal rutin (hasil kosong, lokasi tak dikenal, belum memilih baris) | `DashboardView.xaml.cs`, `ExportView.xaml.cs`, `AlertView.xaml.cs` |
| UX-03 | P1 | Dashboard, Detail, Peta | Label & ambang tingkat tidak sesuai kelas LAPAN: legenda "Sedang (50–79%)", "Rendah (<50%)", sedangkan LAPAN memakai Rendah 0–29 / **Nominal 30–79** / Tinggi 80–100 | `DashboardView.xaml:128-137`, `map.html:54-61,127-129`, `HotspotRow.cs:27-28`, `DetailView.xaml.cs:24-30` |
| UX-04 | P1 | Detail | Judul "Tingkat Keparahan" padahal nilainya keyakinan deteksi satelit (BUG-18) | `DetailView.xaml.cs:27` |
| UX-05 | P2 | Dashboard | Kolom angka diurutkan sebagai teks (BUG-09); daftar tidak punya kolom waktu | `HotspotRow.cs`, `DashboardView.xaml:175-181` |
| UX-06 | P2 | Dashboard | Pencarian hanya 8 lokasi tetap, tanpa saran saat mengetik; salah ketik baru ketahuan setelah tekan Cari (ADD-07) | `DashboardView.xaml.cs` `KnownLocations` |
| UX-07 | P3 | Dashboard | Filter harus ditekan "Terapkan"; tidak ada rentang tanggal (OPS-03) | `DashboardView.xaml:71-88` |
| UX-08 | P1 | Dashboard | Umpan balik memuat lemah: overlay WPF tertutup peta WebView2 (airspace), status hanya teks 11 px; error merah kecil mudah terlewat | `DashboardView.xaml:144-167` |
| UX-09 | P2 | Login | Enter tidak mengirim form (`IsDefault` tidak ada); belum ada jalur **Daftar** & **Masuk sebagai tamu** (K10) | `LoginView.xaml:37-38` |
| UX-10 | P3 | Export | Preview merender seluruh teks (ribuan baris) setiap ganti format; nama file bawaan tanpa tanggal | `ExportView.xaml.cs:44-50,63` |
| UX-11 | P2 | Alert | Alert tidak bisa dibuka ke hotspotnya (peta/detail) | `AlertView.xaml.cs` |
| UX-12 | P3 | Semua | Bahasa campur: "Logout", "Export", "Preview", "Alert" di antara teks Indonesia | semua XAML |
| UX-13 | P2 | Peta, Detail | Tingkat dibedakan terutama lewat warna; kuning & oranye kontras rendah (lihat §2) | `map.html`, `DetailView.xaml.cs` |
| UX-14 | P2 | Detail | Minim konteks: tanpa nama wilayah, tanpa peta kecil, tanpa status verifikasi/navigasi (OPS-01/02) | `DetailView.xaml` |
| UX-15 | P2 | Semua | Teks 10–11 px untuk legenda, petunjuk, status, dan pesan error | `DashboardView.xaml:67,122-137,167`, `LoginView.xaml:42` |
| UX-16 | P3 | Dashboard, header | Keyboard: tidak bisa Enter untuk membuka detail dari daftar; tombol 🔔 tanpa nama aksesibilitas | `DashboardView.xaml` |
| UX-17 | P3 | Dashboard | Baris status terlalu panjang (banyak bagian dipisah "·") untuk dibaca sekilas | `DashboardView.xaml.cs` `SinkronDariFirmsAsync` |
| UX-18 | P1 | Semua (warga) | Istilah teknis (confidence, FRP, MW, UTC) belum dijelaskan untuk Publik/Tamu | Detail, daftar, legenda |

## 2. Kontras warna (WCAG, dihitung 2026-11-22)

| Pasangan | Rasio | Status (teks normal ≥ 4.5, teks besar ≥ 3) |
|---|---|---|
| `MutedText #6D6D6D` di putih | 5.17 : 1 | ✅ |
| `MutedText` di `PageBg #F7F3F0` | 4.69 : 1 | ✅ tipis |
| Putih di `FireRed #B71C1C` (header, tombol utama) | 6.57 : 1 | ✅ |
| Teks tombol sekunder `#333` di `#EDE7E3` | 10.31 : 1 | ✅ |
| `FireRed` (pesan error) di putih | 6.57 : 1 | ✅ |
| Kuning tingkat rendah `#E6C800` di putih (titik/legenda) | 1.66 : 1 | ❌ (elemen grafis butuh ≥ 3) |
| Oranye tingkat sedang `#E68C00` di putih | 2.59 : 1 | ❌ |
| Label Detail "Rendah" `rgb(191,160,15)` di putih | 2.55 : 1 | ❌ |
| Label Detail "Sedang" `#E65100` di putih | 3.79 : 1 | ⚠ hanya lolos untuk teks besar |

## 3. Rencana perbaikan (urut prioritas)

- [x] **UX-01** — `MinWidth/MinHeight` jendela; panel kiri Dashboard dalam `ScrollViewer`. (FE-07)
- [x] **UX-03** — Samakan kelas tingkat dengan LAPAN: **Rendah (< 30) / Nominal (30–79) / Tinggi (≥ 80)** di legenda, daftar, Detail, dan peta, lewat satu sumber `TingkatHotspot` (FE-01). Untuk VIIRS (`l/n/h` = 30/60/90) hasilnya sama; untuk MODIS numerik batas 30 yang berlaku.
- [x] **UX-04** — Ganti "Tingkat Keparahan" dengan "Keyakinan deteksi" + penjelasan singkat. (BUG-18)
- [x] **UX-08** — Status memuat/gagal ditampilkan di luar area peta (bilah status jelas di atas peta/daftar), teks ≥ 12 px; error dengan warna + ikon + teks tebal.
- [x] **UX-18** — Penjelasan istilah untuk warga (tooltip/teks bantu); di tampilan Publik/Tamu pakai bahasa awam (lihat `copy-and-accessibility.md`).
- [x] **UX-02** — Ganti MessageBox rutin dengan pesan di tempat (inline) lewat `PesanUi` (FE-05); MessageBox hanya untuk konfirmasi & error kritis.
- [x] **UX-05** — Urutkan angka secara numerik; tambah kolom "Waktu (lokal)". (BUG-09)
- [x] **UX-09** — `IsDefault` pada tombol Login; tambah tombol "Daftar" dan "Masuk sebagai tamu" (SCR-06/07).
- [x] **UX-11** — Dari halaman Alert bisa membuka hotspot terkait (fokus peta / Detail).
- [x] **UX-13** — Tingkat tidak hanya warna: ukuran/garis tepi berbeda + label teks; warna dengan kontras ≥ 3:1 (diputuskan di sistem desain).
- [x] **UX-14** — Detail: nama wilayah (setelah K2), status verifikasi, navigasi (sesuai peran).
- [x] **UX-15** — Ukuran teks minimum 12 px; legenda 12 px.
- [x] **UX-06** — Saran lokasi saat mengetik / daftar wilayah lengkap (setelah K2/ADD-07).
- [x] **UX-12** — Seragamkan istilah (lihat glosarium di `copy-and-accessibility.md`).
- [x] **UX-16** — Enter membuka Detail dari daftar; `AutomationProperties.Name` untuk tombol ikon.
- [x] **UX-17** — Status dipecah: sumber & waktu sinkron, jumlah titik, hasil simpan/alert (mis. sebagai teks terpisah).
- [x] **UX-07** — Rentang tanggal untuk Admin/Operator (SCR-13); filter otomatis atau tetap tombol (keputusan terbuka).
- [x] **UX-10** — Preview dibatasi (mis. 200 baris pertama + jumlah total); nama file bawaan berisi tanggal.

## 4. Keputusan terbuka
1. Filter langsung berlaku saat slider digeser, atau tetap tombol "Terapkan"? Usulan: tetap tombol (lebih ringan untuk ribuan titik), tapi tampilkan jumlah hasil.
2. Warna tingkat pengganti (kontras ≥ 3:1) diputuskan bersama sistem desain baru.

## 5. Rujukan
`docs/debug/bug-fix.md` (BUG-09, BUG-18), `docs/frontend/code-structure.md`, `docs/ui/design-system.md`, `docs/ui/ux/copy-and-accessibility.md`, Panduan LAPAN Fire Hotspot v2.0 (kelas tingkat kepercayaan)
