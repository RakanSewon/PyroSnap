# Teks Antarmuka & Aksesibilitas

> Status: **pedoman + rencana**. Disusun 2026-11-22.

## 1. Prinsip teks
1. **Bahasa Indonesia baku dan ringkas** untuk semua label, tombol, dan pesan.
2. **Pesan menjelaskan apa yang terjadi + apa yang bisa dilakukan**, mis. "Database tidak tersedia. Coba lagi beberapa saat lagi."
3. **Dua tingkat bahasa**: istilah teknis boleh untuk Admin/Operator; untuk Publik/Tamu pakai bahasa awam dan selalu dengan penjelasan.
4. **Waktu**: tampilkan waktu lokal; UTC hanya sebagai keterangan tambahan (sudah dipakai di Detail).
5. **Jangan menakut-nakuti**: hotspot adalah deteksi satelit, belum tentu kebakaran; tulis "titik panas terdeteksi", bukan "kebakaran terjadi", kecuali sudah diverifikasi lapangan (OPS-01).

## 2. Glosarium (usulan, untuk UX-12)

| Sekarang | Usulan | Catatan |
|---|---|---|
| Hotspot | Titik panas (hotspot) | Pertama kali muncul ditulis lengkap |
| Confidence | Keyakinan deteksi | Kelas: Rendah / Nominal / Tinggi (LAPAN) |
| Tingkat Keparahan | Keyakinan deteksi | BUG-18 / UX-04 |
| FRP (MW) | Intensitas api (FRP, MW) | Untuk Publik: "intensitas api" saja + tooltip |
| Logout | Keluar | – |
| Export / Export Laporan | Ekspor / Ekspor Laporan | – |
| Preview | Pratinjau | – |
| Alert | Peringatan (Admin/Operator: "Peringatan operasional") | Tombol 🔔 tetap |
| Muat Ulang Data | Perbarui Data dari NASA FIRMS | Lebih jelas sumbernya |

## 3. Teks peringatan

| Untuk | Contoh | Sumber aturan |
|---|---|---|
| Admin/Operator (sudah ada) | "Hotspot kepercayaan tinggi (90%) di -4.380, 122.421 · FRP 27.84 MW · 21 Nov 2026 06:05 UTC · segera penanggulangan" | `AmbangKritis.BuatPesan` |
| Publik/Tamu (baru, PUB-01) | "Waspada: ada titik panas berkeyakinan tinggi terdeteksi di sekitar {wilayah} pada {waktu lokal}. Hindari membakar lahan dan ikuti informasi resmi setempat. Lihat Panduan." | Usulan; disetujui tim sebelum dipakai |

Isi halaman Panduan & kontak darurat (PUB-03) **harus diambil dari sumber resmi dan dicek tim**; nomor kontak tidak ditulis dari ingatan.

## 4. Aksesibilitas (target)

| ID | Target | Keterangan |
|---|---|---|
| A11Y-01 | Teks minimum 12 px; status/error ≥ 12 px | UX-15 |
| A11Y-02 | Kontras teks ≥ 4.5:1, elemen grafis (titik peta, legenda) ≥ 3:1 | Lihat tabel kontras di `ux-audit.md` §2; warna tingkat kuning/oranye perlu diganti (sistem desain) |
| A11Y-03 | Informasi tidak hanya lewat warna | Tingkat juga ditulis sebagai teks + beda ukuran/garis tepi (UX-13) |
| A11Y-04 | Bisa dipakai dengan keyboard | Enter untuk Login/Daftar (IsDefault), Esc untuk kembali, Enter membuka Detail dari daftar, urutan Tab logis |
| A11Y-05 | Nama aksesibilitas untuk tombol ikon | `AutomationProperties.Name` pada 🔔 dan tombol ikon lain |
| A11Y-06 | Pesan status bisa dibaca pembaca layar | `AutomationProperties.LiveSetting` pada bilah status (bila memungkinkan di WPF .NET 8) |
| A11Y-07 | Peta bukan satu-satunya cara | Semua informasi peta juga ada di daftar (sudah terpenuhi) |

## 5. Checklist target
- [ ] **CPY-01** — Tim menyetujui glosarium §2, lalu teks UI diseragamkan (UX-12). *(2026-11-22: glosarium sudah diterapkan di semua layar; menunggu persetujuan tim, mudah diubah bila tidak setuju.)*
- [ ] **CPY-02** — Tim menyetujui teks peringatan warga §3 sebelum PUB-01 dikerjakan. *(2026-11-22: teks §3 sudah dipakai di banner `DashboardView.Warga.cs`; menunggu persetujuan tim.)*
- [ ] **CPY-03** — Isi Panduan & kontak darurat dari sumber resmi, dicek minimal 2 anggota, tanggal pengecekan dicatat. *(2026-11-22: isi diringkas dari Kemenkes, Permenkominfo 10/2016 + liputan 112, BNPB 117, rangkuman nomor darurat Katadata, dan LAPAN; dicek oleh Claude, belum dicek anggota tim.)*
- [x] **CPY-04** — Semua pesan error mengikuti pola "apa yang terjadi + apa yang bisa dilakukan".
- [ ] **A11Y-A1** — A11Y-01…A11Y-07 diterapkan dan dicek di Windows (Narrator untuk A11Y-05/06).

## 6. Rujukan
`docs/ui/ux/ux-audit.md`, `docs/ui/design-system.md`, `PyroSnap/Services/AmbangKritis.cs`, Panduan LAPAN Fire Hotspot v2.0
