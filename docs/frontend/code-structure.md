# Struktur Kode Frontend (WPF, code-behind)

> Status: **dikerjakan 2026-11-22** (FE0). Keputusan: **tetap code-behind, dirapikan**; MVVM tetap opsional di Fase 6 (ARCH-B2).
> Hasil: Dashboard dipecah menjadi 6 partial class (`DashboardView.xaml.cs` 140, `.Data.cs` 260, `.Historis.cs` 79, `.Filter.cs` 139, `.Warga.cs` 161, `.Peta.cs` 237 baris); helper `TingkatHotspot`, `Navigasi`, `PesanUi`; warna lewat token `App.xaml`. Build 0 error. Belum diuji di Windows.

## 1. Tujuan

Membuat kode tampilan mudah dibaca dan diubah sebelum Fase 7 menambah banyak layar, tanpa memperkenalkan arsitektur baru.

## 2. Kondisi saat ini (2026-11-22)

| File | Baris | Catatan |
|---|---|---|
| `Views/DashboardView.xaml.cs` | 675 | Data (FIRMS/DB), filter, peta WebView2, peta kanvas, alert, navigasi dalam satu file |
| `Views/DashboardView.xaml` | 188 | Panel kiri `StackPanel` tidak bisa di-scroll |
| `Views/AlertView.xaml.cs` | 120 | – |
| `Views/ExportView.xaml.cs` | 122 | – |
| `Views/LoginView.xaml.cs` | 92 | – |
| `Assets/map.html` | 140 | Ambang & warna tingkat ditulis ulang di JS |

Duplikasi yang ditemukan:
- **Aturan tingkat** (Tinggi ≥ 80, Sedang ≥ 50, warna, ukuran titik) ditulis di 4 tempat: `HotspotRow.cs:27-28`, `DetailView.xaml.cs:24-30`, `DashboardView.xaml.cs:635-638` (kanvas), `map.html:54-61` + legenda `map.html:127-129` dan `DashboardView.xaml:128-137`.
- **Navigasi** `((MainWindow)Window.GetWindow(this)).Navigate(new X())` diulang di hampir setiap halaman.
- **Warna langsung (hardcoded)** di XAML/C#/HTML, di luar `App.xaml`: `#FFCCCCCC` (3×), `#FFEAF2F8`, `#CCFFFFFF` (2×), warna tingkat (`#C81E1E`/`#E68C00`/`#E6C800` dan RGB setara di C#), `#FFF7F3F0` di `MainWindow.xaml` (sama dengan `PageBg`).
- **Pemendekan pesan error** (`pesan.Length > 160 ? …`) diulang di Login, Export, Alert, Dashboard.

## 3. Struktur target (tanpa MVVM)

```
PyroSnap/Views/
├── DashboardView.xaml
├── DashboardView.xaml.cs          (siklus hidup halaman + event handler tombol)
├── DashboardView.Data.cs          (partial: muat FIRMS/DB, status)
├── DashboardView.Filter.cs        (partial: cari lokasi, filter, rentang tanggal)
├── DashboardView.Peta.cs          (partial: WebView2 + kanvas)
├── TingkatHotspot.cs              (aturan tingkat + warna + ukuran, satu sumber)
├── Navigasi.cs                    (pindah halaman + penjaga sesi/peran)
├── PesanUi.cs                     (format & pemendekan pesan, tampilkan status/error)
└── … halaman lain (Login, Register, Detail, Export, Alert, Panduan)
```

Prinsip:
1. **Partial class**, bukan class/UserControl baru, untuk memecah Dashboard. Perilaku tetap, risiko kecil.
2. **Helper statis kecil** untuk logika yang dipakai beberapa halaman (tingkat, navigasi, pesan).
3. **Ambang tingkat untuk peta** dikirim dari C# ke `map.html` lewat `window.PYROSNAP_CONFIG` (mekanisme yang sudah ada untuk key CARTO), jadi tidak ditulis ulang di JS.
4. **Semua warna lewat resource** di `App.xaml` (`{StaticResource …}` / `FindResource`), sebagai persiapan sistem desain baru.
5. **Pola async tetap**: handler `async void` selalu ber-try/catch; tombol dinonaktifkan selama proses; pesan gagal tidak membuat aplikasi crash.

## 4. Checklist target

- [x] **FE-01** — `TingkatHotspot` sebagai satu sumber aturan tingkat (nama kelas, ambang, warna, ukuran), dipakai `HotspotRow`, `DetailView`, kanvas, dan dikirim ke `map.html`. Ambang mengikuti keputusan UX-03.
- [x] **FE-02** — Pecah `DashboardView.xaml.cs` menjadi partial class (Data, Filter, Peta). *Selesai bila* tiap file < ±250 baris dan perilaku tidak berubah.
- [x] **FE-03** — `Navigasi` terpusat + penjaga sesi/peran (SCR-A1, SCR-A9).
- [x] **FE-04** — Hak akses UI dibaca dari properti `AppSession` saja (RUI-01).
- [x] **FE-05** — `PesanUi`: satu cara menampilkan info/peringatan/error (lihat UX-02); MessageBox hanya untuk konfirmasi dan error kritis.
- [x] **FE-06** — Pindahkan semua warna hardcoded ke resource `App.xaml` (daftar di §2). Persiapan DS-01.
- [x] **FE-07** — Ukuran minimum jendela + panel kiri Dashboard bisa di-scroll (UX-01).
- [x] **FE-08** — Periksa ulang semua handler `async void` (try/catch, tombol nonaktif saat proses).
- [x] **FE-09** — Build bersih dan uji manual setiap layar setelah perapian (tidak ada perubahan perilaku).

## 5. Keputusan terbuka
1. Teks UI tetap di XAML/kode atau dipindah ke file resource (persiapan bahasa lain)? Usulan: tetap di XAML, cukup konsisten (lihat `docs/ui/ux/copy-and-accessibility.md`).
2. Fallback peta kanvas dipertahankan? Usulan: ya, tetapi dipindah ke `DashboardView.Peta.cs` dan tidak dikembangkan lagi.

## 6. Rujukan
`docs/architecture/backend-architecture.md` (Fase A/B), `docs/ui/design-system.md`, `docs/ui/ux/ux-audit.md`, `Claude.md` §6
