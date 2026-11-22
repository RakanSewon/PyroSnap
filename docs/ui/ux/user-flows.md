# Alur Pengguna (User Flow)

> Status: **rencana** untuk Fase 7. Disusun 2026-11-22. Layar mengacu ID di `docs/frontend/screens-and-navigation.md`.

## 1. Persona

| Persona | Peran | Kebutuhan utama | Konteks pemakaian |
|---|---|---|---|
| Admin (pengelola & analis data) | Admin | Data terbaru tersinkron, alert terkirim, laporan | Kantor, rutin beberapa kali sehari |
| Petugas posko/lapangan | Operator | Tahu titik kritis, menuju lokasi, melaporkan hasil cek | Posko/lapangan, butuh cepat dan jelas |
| Warga terdaftar | Publik | Tahu risiko di wilayahnya, apa yang harus dilakukan | Sesekali, saat musim kemarau/asap |
| Warga tanpa akun | Tamu | Lihat cepat tanpa mendaftar | Sekali lihat |

## 2. Alur

### FLOW-01 — Warga masuk sebagai tamu
1. Login (SCR-01) → **Masuk sebagai tamu**.
2. Dashboard mode Tamu: peta + daftar 24 jam terakhir dari database, ringkasan risiko per wilayah (SCR-09), tombol **Panduan**.
3. Pilih wilayah (SCR-08, hanya untuk sesi ini) → banner peringatan berbahasa awam bila ada titik berkeyakinan tinggi di wilayah itu.
4. Buka Detail → lihat penjelasan istilah dan **status verifikasi lapangan** (bila ada).
5. **Keluar** → kembali ke Login. Tidak ada data yang tersimpan.
- Gagal: database tidak tersedia → pesan jelas "Data belum bisa dimuat, coba lagi nanti", Panduan tetap bisa dibuka.

### FLOW-02 — Warga mendaftar (Publik)
1. Login → **Daftar** (SCR-06): username, password, konfirmasi password.
2. Validasi di tempat: username sudah dipakai / password < 8 karakter / konfirmasi tidak sama.
3. Berhasil → langsung masuk sebagai Publik → diminta memilih **wilayah pilihan** (bisa dilewati).
4. Dashboard: banner peringatan untuk wilayahnya, ringkasan risiko, peta.
5. **Export** laporan tersedia (K10); tercatat atas nama akunnya.
- Data pribadi minimal: hanya username + hash password (tanpa email/telepon).

### FLOW-03 — Operator menindaklanjuti alert
1. Login Operator → Dashboard (data dari database) → badge **🔔 Alert (n)**.
2. Buka Alert (SCR-05) → pilih alert → **buka hotspotnya** (UX-11) → Detail.
3. Detail: **Navigasi ke titik** (salin koordinat / buka arah) (SCR-12).
4. Setelah cek lapangan: **Verifikasi** → pilih *api aktif / sudah padam / bukan kebakaran* + catatan → simpan (SCR-11).
5. Status verifikasi tampil untuk semua peran; alert ditandai dibaca.
6. Opsional: **data historis** (SCR-13) untuk melihat titik di area yang sama beberapa hari terakhir; export laporan.

### FLOW-04 — Admin menyinkronkan data
1. Login Admin → Dashboard otomatis sinkron dari NASA FIRMS → simpan ke DB → alert untuk titik kritis (sudah ada sejak F4/F5).
2. Status menunjukkan jumlah titik, titik baru, dan hotspot kritis.
3. **Muat Ulang Data** bila perlu; bila FIRMS gagal, data terakhir dari DB tetap tampil.
4. (⏳) Kelola akun dan riwayat laporan, bila SCR-14/15 disetujui.

### FLOW-05 — Kondisi gagal yang harus jelas bagi semua
| Kondisi | Yang dilihat pengguna |
|---|---|
| Database mati saat Login/Daftar/Tamu | Pesan "Database tidak tersedia" + saran coba lagi; Panduan tetap bisa dibuka |
| FIRMS gagal (Admin) | Data terakhir dari DB + pesan alasan |
| Database kosong (Operator/Publik/Tamu) | "Belum ada data. Data diperbarui oleh petugas." |
| Tidak ada hotspot di wilayah pilihan | Pesan positif: "Tidak ada titik panas terdeteksi di {wilayah} dalam 24 jam terakhir." |

## 3. Checklist target
- [ ] **FLOW-A1** — Setiap alur di §2 bisa dijalankan ujung ke ujung di Windows (dicatat di `testing-schema.md`).
- [ ] **FLOW-A2** — Setiap kondisi gagal di FLOW-05 memberi pesan sesuai tabel.
- [ ] **FLOW-A3** — Uji singkat dengan 1–2 orang di luar tim (mis. teman sebagai "warga") untuk FLOW-01 dan FLOW-02; catat kebingungan yang muncul.

## 4. Rujukan
`docs/frontend/screens-and-navigation.md`, `docs/frontend/role-based-ui.md`, `docs/ui/ux/copy-and-accessibility.md`
