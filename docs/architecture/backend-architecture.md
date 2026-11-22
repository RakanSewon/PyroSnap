# Arsitektur Backend PyroSnap

> Status dokumen: **rencana** (belum ada kode yang diubah). Disusun 2026-11-22.
> Sumber: `Claude.md`, `Diagram_Pyro_Snap.drawio.png` (8 class), `log.md`, hasil review kode.

## 1. Tujuan

Memberi PyroSnap struktur backend yang rapi dan bisa diuji, **bertahap**:

- **Fase A (wajib)** – rapikan sebatas 8 class diagram. Tanpa DI, tanpa MVVM, tanpa interface per service.
- **Fase B (opsional, menunggu persetujuan tim)** – Service + interface, MVVM, DI, unit test sesuai `Claude.md` §6–8.

Alasan bertahap: `log.md` mencatat implementasi penuh (3 project, DI, 101 test) pernah ditolak sebagai *over-engineering*. Apa pun di Fase B **tidak boleh dikerjakan sebelum disetujui**.

## 2. Kondisi saat ini

### 2.1 Pemetaan class diagram → file

| Class diagram | File sekarang | Masalah |
|---|---|---|
| Location | `PyroSnap/Models/Location.cs` | – |
| Hotspot | `PyroSnap/Models/Hotspot.cs` | `GetDetail()` memakai culture sistem (`Hotspot.cs:23-29`) |
| FilterCriteria | `PyroSnap/Models/FilterCriteria.cs` | `Wilayah` tidak pernah dievaluasi; UI hanya memakai confidence & FRP |
| HotspotReport | `PyroSnap/Services/HotspotReport.cs` | Ada di folder `Services` tapi namespace `PyroSnap`; `isi` privat sehingga `ExportView` membangun ulang teks yang sama (`ExportView.xaml.cs:40-55`) |
| HotspotDataSource | `PyroSnap/Services/HotspotDataSource.cs` | Parser salah indeks, fallback dummy diam-diam (lihat `debug/bug-fix.md`) |
| User / Account | `PyroSnap/src/User.cs` | Password plaintext; tidak terhubung ke tabel `users`; login UI punya jalur cadangan hardcoded |
| Notification / Alert | `PyroSnap/src/Alert.cs` | Hanya `Console.WriteLine` (tidak terlihat di WPF); tidak pernah dipicu |
| Dashboard / MapViewer | `PyroSnap/src/MapViewer.cs` | Kode mati (console); perannya sudah diambil `Views/DashboardView` + Leaflet |

### 2.2 Komponen di luar diagram (sudah ada)

| Komponen | File | Catatan |
|---|---|---|
| Koneksi DB | `Services/DatabaseConnection.cs` | Tidak dipanggil dari mana pun; hanya baca env `PYROSNAP_DB`, default tanpa password |
| State antar-halaman | `Views/AppSession.cs` | Static global; tidak menyimpan filter/lokasi |
| Pembungkus baris grid | `Views/HotspotRow.cs` | Hanya presentasi |
| Navigasi | `MainWindow.Navigate` | Tiap navigasi membuat `new DashboardView()` sehingga data diunduh ulang |

### 2.3 Alur data sekarang

```
NASA FIRMS (CSV) ──HttpClient──▶ HotspotDataSource.ParseCsvToHotspots
        │ (gagal → 1 titik dummy, tanpa pesan)
        ▼
DashboardView (code-behind) ──FilterCriteria.IsMatch──▶ _tampil
        ├──▶ Leaflet (WebView2)      ├──▶ DataGrid      ├──▶ AppSession.CurrentHotspots ──▶ ExportView ──▶ HotspotReport / CSV
```

## 3. Target Fase A (sebatas diagram)

### 3.1 Struktur folder target

```
PyroSnap/
├── Models/        Hotspot, Location, FilterCriteria, HotspotReport, User, Alert
├── Services/      HotspotDataSource, AppConfig*, DatabaseConnection,
│                  HotspotRepository*, UserRepository*, ReportRepository*, AlertRepository*
├── Views/         *View.xaml(.cs), HotspotRow, AppSession
└── Assets/        map.html (+ Leaflet lokal)
```
`*` = infrastruktur di luar diagram, **class konkret tanpa interface**, hanya dibuat saat fase DB yang bersangkutan dikerjakan.

### 3.2 Prinsip

1. Model murni: tidak memanggil UI, DB, atau jaringan.
2. `HotspotDataSource` memproduksi `Hotspot` (arah dependency data source → hotspot, `Claude.md` §5).
3. `User` tidak memanggil `FilterCriteria`/`HotspotDataSource`/`HotspotReport` langsung; pengecekan peran dilakukan di lapisan view (Fase A) atau ViewModel (Fase B).
4. `Hotspot` tidak memanggil `Alert`; ambang kritis dievaluasi pemanggil (Fase A) atau `INotificationService` (Fase B).
5. Semua I/O `async`; tidak ada `async void` tanpa try/catch kecuali event handler UI yang membungkus try/catch.
6. Tidak ada rahasia (API key, password DB) di kode; dibaca lewat `AppConfig`.

## 4. Checklist target

### Fase A – Struktur & kebersihan
- [ ] **ARCH-01** — Pindahkan `User.cs`, `Alert.cs` ke `Models/`; `HotspotReport.cs` ke `Models/`. *Selesai bila* semua file di folder yang sesuai dan `dotnet build` tanpa error.
- [ ] **ARCH-02** — Seragamkan namespace (`PyroSnap.Models`, `PyroSnap.Services`, `PyroSnap.Views`). *Selesai bila* tidak ada file dengan namespace root `PyroSnap` untuk class domain.
- [ ] **ARCH-03** — Putuskan nasib `MapViewer.cs` (hapus atau hubungkan ke `DashboardView`) lalu hapus kode mati. *Selesai bila* tidak ada class yang tidak dipakai dan keputusan dicatat di `log.md`.
- [ ] **ARCH-04** — Hapus folder `src/` di root repo (sisa `bin`/`obj`). *Selesai bila* `git status` bersih dan build tetap jalan.
- [x] **ARCH-05** — Buat `AppConfig` (baca env + file `.env` lokal) untuk `PYROSNAP_DB`, key FIRMS, dll. *Selesai bila* tidak ada string rahasia di kode dan `DatabaseConnection` memakai `AppConfig`.
- [x] **ARCH-06** — `FilterCriteria` jadi satu sumber kebenaran filter (waktu, confidence, FRP, wilayah) yang dipakai pencarian + slider sekaligus. *Selesai bila* urutan "cari lalu filter" dan "filter lalu cari" menghasilkan daftar sama.
- [x] **ARCH-07** — Ganti pola `new DashboardView()` pada navigasi dengan penyimpanan state (data, filter, pilihan) di `AppSession` atau reuse instance di `MainWindow`. *Selesai bila* kembali dari Detail/Export tidak memicu unduhan ulang dan filter bertahan.
- [x] **ARCH-08** — `HotspotReport` menyediakan teks hasil `Generate()` (properti baca-saja) agar `ExportView` tidak membangun ulang. *Selesai bila* preview dan file export memakai teks dari satu sumber.
- [ ] **ARCH-09** — Tentukan aturan penanganan error lintas lapisan (service melempar exception spesifik / mengembalikan hasil gagal; view yang menampilkan pesan). *Selesai bila* tidak ada `catch { }` kosong dan tak ada data dummy tersembunyi.
- [ ] **ARCH-10** — Dokumentasikan alur data final (diagram teks) di dokumen ini dan perbarui §2.3. *Selesai bila* alur sesuai kode.

### Fase B – Opsional (menunggu persetujuan)
- [ ] **ARCH-B1** — `IHotspotDataSource`, `IAuthService`, `INotificationService` + implementasi. *Syarat:* disetujui tim.
- [ ] **ARCH-B2** — MVVM (`MapViewModel`, `LoginViewModel`, `FilterViewModel`, `RelayCommand`, `ObservableObject`). *Syarat:* disetujui tim.
- [ ] **ARCH-B3** — Dependency Injection (`Microsoft.Extensions.DependencyInjection`). *Syarat:* ARCH-B1 selesai.
- [ ] **ARCH-B4** — Unit test (`FilterCriteriaTests`, `HotspotDataSourceTests`, `AuthServiceTests`, `NotificationServiceTests`). *Syarat:* ARCH-B1 selesai. Lihat `debug/testing-schema.md`.

## 5. Keputusan terbuka

1. **Nasib `MapViewer.cs`** – hapus (peran sudah diambil `DashboardView`) atau dipertahankan sebagai penampung `pin`/`filter` yang dipakai `DashboardView`? (log.md butir 4: diagram tak punya penampung hotspot.)
2. **Atribut wilayah pada Hotspot** – tambahkan (mis. lewat `location_id`/reverse lookup) atau hapus `Wilayah` dari `FilterCriteria`? (log.md butir 1.)
3. **Repository class** – apakah `HotspotRepository` dkk dihitung "tambahan di luar diagram"? Tanpanya, query SQL tersebar di view.
4. **Cara menjaga state dashboard** – reuse instance `DashboardView` (paling kecil) atau pindah state ke `AppSession`.

## 6. Rujukan

- `Claude.md` §4–§8, `log.md`, `docs/architecture/database-architecture.md`, `docs/debug/bug-fix.md`, `docs/progress/backend-progress.md`
