# Backend: Laporan (HotspotReport) & Alert

> Status: **rencana**. Kode belum diubah. Disusun 2026-11-22.

## 1. Tujuan

- `HotspotReport` menghasilkan dan mengekspor laporan dengan benar, dan tiap laporan tercatat di database.
- `Alert` dipicu ketika hotspot melewati ambang kritis, ditampilkan di UI, dan tersimpan.

---

## Bagian A — HotspotReport

### A.1 Kondisi saat ini
- `HotspotReport` (`Services/HotspotReport.cs`): `Generate()` menyusun teks, `ExportToFile(path)` menulis `isi` (privat) lewat `File.WriteAllText` tanpa penanganan error.
- `ExportView` membangun ulang teks TXT sendiri karena `isi` tak bisa dibaca, dan membuat CSV di layer UI (`ExportView.xaml.cs:40-68`).
- Angka CSV mengikuti culture sistem (BUG-07); error tulis file tak ditangani (BUG-16).
- Tidak ada pencatatan ke `hotspot_report` / `report_hotspot`; format `TXT` bahkan akan ditolak CHECK skema (BUG-11).
- Format PDF/XLSX ada di skema tetapi tidak diimplementasikan (ADD-05).

### A.2 Perilaku target
1. `HotspotReport(format)` mendukung `TXT` dan `CSV` pada fase wajib.
2. `Generate()` menghasilkan teks laporan; teks bisa dibaca (properti baca-saja) sehingga preview dan file memakai satu sumber (ARCH-08).
3. `ExportToFile(path)` menangani error I/O (melempar exception spesifik yang ditangkap view).
4. Setelah ekspor berhasil: simpan `hotspot_report` (user, format, tanggal, `file_path`) dan baris `report_hotspot` untuk tiap hotspot dalam satu transaksi.
5. Hanya peran berhak yang mengekspor (FT-05e).
6. `report_hotspot.hotspot_id` memerlukan hotspot sudah tersimpan di DB; `Hotspot` pada diagram tidak punya id, sehingga repository mencari id lewat kunci alami (source, lat, lon, waktu) — butuh SCH-01.

### A.3 Checklist
- [x] **RPT-01** — `HotspotReport` menyediakan teks hasil `Generate()` (baca-saja); `ExportView` memakainya. *Selesai bila* tidak ada kode penyusun teks ganda. (ARCH-08)
- [x] **RPT-02** — Format CSV dipindah/dibuat konsisten di `HotspotReport` dengan `InvariantCulture`. (BUG-07, TST-14)
- [x] **RPT-03** — `ExportToFile` aman terhadap error I/O. (BUG-16, TST-15)
- [x] **RPT-04** — Terapkan SCH-03 (format `TXT` diterima) lalu `ReportRepository.SaveAsync(report, userId, filePath)` dalam satu transaksi. (DBA-07, TST-24/25)
- [x] **RPT-05** — Pemetaan `Hotspot` → `hotspot_id` untuk `report_hotspot` (lookup kunci alami) teruji. *Selesai bila* jumlah baris `report_hotspot` = jumlah hotspot dalam laporan.
- [x] **RPT-06** — Export hanya untuk peran berhak. (AUTH-06)
- [x] **RPT-07** — Laporan kosong ditolak dengan pesan inline, tidak menulis file/DB.
- [ ] **RPT-B1** — (Opsional) PDF/XLSX. (ADD-05)

### A.4 Keputusan terbuka
1. *(Diputuskan 2026-11-22)* `file_path` disimpan apa adanya; bila lebih dari 255 karakter disimpan NULL.
2. Laporan menyimpan *snapshot* hotspot lewat `report_hotspot` (agregasi, sesuai diagram) — pastikan hotspot tidak ikut terhapus saat laporan dihapus (sesuai ON DELETE CASCADE di junction saja).

---

## Bagian B — Alert

### B.1 Kondisi saat ini
- `Alert` (`src/Alert.cs`): `SendAlert(targetUser)` hanya `Console.WriteLine` (tidak terlihat di aplikasi WPF); tidak pernah dipanggil dari alur aplikasi (BUG-17).
- Tabel `alert` ada (`hotspot_id`, `target_user_id`, `message`, `timestamp`, `is_read`) tetapi tak dipakai.
- Tidak ada definisi ambang kritis.

### B.2 Perilaku target
1. **Ambang kritis** tunggal (konfigurasi/konstanta di satu tempat): mis. confidence ≥ nilai-A dan/atau FRP ≥ nilai-B (nilai menunggu keputusan domain).
2. **Pemicu:** setelah sync sukses, tiap hotspot baru dievaluasi; yang melewati ambang menghasilkan `Alert` untuk penerima yang ditetapkan. `Hotspot` **tidak** memanggil `Alert` (`Claude.md` §4.7).
3. **Anti duplikat:** satu alert per (hotspot, penerima); sync ulang tidak membuat alert baru (butuh SCH-08).
4. **Tampilan:** jumlah belum dibaca (badge) + daftar alert di UI; `MarkAsRead()` mengubah `IsRead` dan `is_read`.
5. **Penyimpanan:** `AlertRepository.InsertAsync`, `ListUnreadAsync(userId)`, `MarkReadAsync(alertId)`.
6. Fase A: evaluasi dilakukan oleh pemanggil sederhana (mis. fungsi di layer sync); Fase B: `INotificationService.Evaluate()` + event `AlertRaised`.

### B.3 Checklist
- [x] **ALR-01** — Tetapkan ambang kritis (nilai dan logika AND/OR) dan catat di `feature/main-feature.md` FT-07a.
- [x] **ALR-02** — Fungsi evaluasi `Hotspot` → perlu alert atau tidak, tanpa ketergantungan UI. *Selesai bila* uji data di atas/bawah ambang berperilaku benar. (TST-34/35)
- [x] **ALR-03** — `Alert.SendAlert()` tidak lagi memakai `Console.WriteLine`; hasilnya bisa dikonsumsi UI. (BUG-17)
- [x] **ALR-04** — Pemicu dipanggil setelah sync sukses (butuh BE-09 dan F4).
- [x] **ALR-05** — Terapkan SCH-08 (kunci unik alert) dan `AlertRepository` (insert/list/mark-read). (DBA-08, TST-36)
- [x] **ALR-06** — UI menampilkan badge dan daftar alert; `MarkAsRead()` bekerja. (FT-07c)
- [x] **ALR-07** — Batas jumlah alert per sync (hindari ratusan sekaligus) dan ringkasannya.
- [ ] **ALR-B1** — (Opsional) `INotificationService` + event `AlertRaised`. (ARCH-B1)

### B.4 Keputusan terbuka
1. *(Diputuskan 2026-11-22)* Ambang kelas tinggi LAPAN: confidence ≥ 80 (VIIRS `h`) = "segera penanggulangan". FRP tidak dipakai sebagai syarat (tidak ada ambang standar di literatur), hanya dicantumkan di pesan. Didefinisikan di `Services/AmbangKritis.cs`.
2. *(Diputuskan 2026-11-22)* Semua user (Admin, Analis, Operator) menerima alert.
3. *(Diputuskan 2026-11-22)* `AlertId` tetap string sesuai diagram, berisi angka `alert_id` dari tabel.
4. Notifikasi di luar aplikasi (email/Windows toast) — di luar lingkup sampai disetujui.

## Rujukan
`PyroSnap/Services/HotspotReport.cs`, `PyroSnap/src/Alert.cs`, `PyroSnap/Views/ExportView.xaml.cs`, `database/001_schema.sql`, `docs/database/schema-changes.md`, `docs/feature/main-feature.md`
