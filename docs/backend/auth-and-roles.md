# Backend: Autentikasi & Peran (User / Account)

> Status: **rencana**. Kode belum diubah. Disusun 2026-11-22.
> Tidak ada password atau hash nyata di dokumen ini.

## 1. Tujuan

`User.Login()` / `Logout()` pada diagram bekerja dengan akun di tabel `users`, password tersimpan sebagai **hash**, dan peran (Admin/Analis/Operator) menentukan aksi yang boleh dilakukan.

## 2. Kondisi saat ini

- `PyroSnap/src/User.cs`: `Password` disimpan plaintext di memori dan dibandingkan langsung.
- `LoginView.xaml.cs`: akun demo tertanam (`:14-15`), membuat `User` baru tiap klik lalu menambah `|| (username == DemoUser && ...)` sehingga hasil `User.Login` tak berarti (`:35`) (BUG-10).
- Tabel `users` ada dan kosong; tidak ada kode yang membacanya.
- Peran tidak dipakai di UI; logout hanya berpindah halaman tanpa membersihkan `AppSession`.

## 3. Rancangan target (Fase A, sebatas diagram)

```
LoginView ──username,password──▶ UserRepository.FindByUsernameAsync ──▶ User (id, username, hash, role)
                                           │
                       User.Login(username, pass) ── verifikasi hash ──▶ true/false
                                           │ true
                                  AppSession.CurrentUser ──▶ dashboard (tombol sesuai peran)
```

- Signature diagram dipertahankan: `bool Login(string username, string pass)` dan `void Logout()`.
- Atribut `password` diagram → menyimpan **hash** (penyesuaian keamanan, `Claude.md` §4.6).
- Peran sebagai string (sesuai diagram) atau enum `UserRole` (sesuai `Claude.md`) — lihat keputusan terbuka.
- Pengecekan peran dilakukan pemanggil (view) lewat `AppSession.CurrentUser.Role`, **bukan** `User` yang memanggil `HotspotDataSource` dsb. (`Claude.md` §5 catatan penting).
- Pesan gagal login generik (tidak membocorkan username ada/tidak).

## 4. Checklist target

### Fase 3
- [x] **AUTH-01** — `UserRepository.FindByUsernameAsync` (query berparameter). *Selesai bila* mengembalikan user atau `null`. (DBA-06)
- [x] **AUTH-02** — Pilih dan terapkan algoritma hash (lihat keputusan terbuka) untuk membuat dan memverifikasi hash. *Selesai bila* verifikasi benar/salah teruji dan hash berbeda untuk password sama (salt).
- [x] **AUTH-03** — `User.Login()` memverifikasi hash; plaintext tidak disimpan di properti mana pun. *Selesai bila* `Password` tak berisi teks asli. (ACC-10)
- [x] **AUTH-04** — `LoginView` memakai `UserRepository` + `User.Login`; hapus akun demo dan kondisi `||`. *Selesai bila* kredensial demo lama ditolak. (BUG-10, ACC-09)
- [x] **AUTH-05** — `AppSession.CurrentUser` menyimpan user aktif; `Logout()` membersihkan user, hotspot terpilih, dan state filter. (FT-06c, ACC-08)
- [x] **AUTH-06** — Tombol Sync/Export aktif/nonaktif sesuai matriks hak akses (`docs/database/acount-testing.md` §5). (ACC-06/07)
- [x] **AUTH-07** — Pesan gagal login generik; input kosong divalidasi. (ACC-03/04/05)
- [x] **AUTH-08** — Kegagalan DB saat login menampilkan pesan "database tidak tersedia", bukan "password salah". (TST-27)
- [x] **AUTH-09** — Tidak ada jalur masuk tanpa login: halaman Dashboard/Detail/Export tak bisa dibuka lewat navigasi Kembali setelah logout. (ACC-08)

### Opsional (menunggu persetujuan)
- [ ] **AUTH-B1** — `IAuthService` + `AuthService` (Fase B, ARCH-B1).
- [ ] **AUTH-B2** — Penguncian sementara setelah N kali gagal (ACC-11). Tambahan di luar diagram.
- [ ] **AUTH-B3** — Pendaftaran/pengelolaan akun lewat aplikasi (fase awal: lewat DB saja).

## 5. Keputusan
1. **Algoritma hash — diputuskan 2026-11-22: BCrypt** (paket `BCrypt.Net-Next` 4.0.3). Akun uji di-hash oleh PostgreSQL (`pgcrypto`, `crypt(..., gen_salt('bf', 11))`); format `$2a$` diverifikasi kompatibel dengan BCrypt.Net.
2. `Role`: **tetap string** seperti diagram; nilainya dibatasi CHECK di DB. Hak akses dibaca lewat `AppSession.BolehSinkron`.
3. `User` memegang `PasswordHash` (setter saja, getter privat) dan memverifikasi di `User.Login()` sesuai signature diagram; tidak ada service tambahan di Fase A.
4. Sesi berlaku sampai logout atau aplikasi ditutup. Riwayat navigasi `Frame` dihapus tiap pindah halaman, jadi Back/Alt+← tidak membuka dashboard setelah logout.

### Implementasi (2026-11-22)
- `PyroSnap/src/User.cs` — `PasswordHash`, `SudahLogin`, verifikasi BCrypt; hash rusak = gagal login, bukan crash.
- `PyroSnap/Services/UserRepository.cs` — `FindByUsernameAsync` berparameter.
- `PyroSnap/Views/LoginView.xaml.cs` — validasi input, pesan generik, pesan khusus saat DB tidak tersedia, verifikasi di luar thread UI.
- `PyroSnap/Views/AppSession.cs` — `CurrentUser`, `BolehSinkron`, `Bersihkan()` memanggil `Logout()`.
- `PyroSnap/Views/DashboardView.*` — tanpa login dialihkan ke Login; nama + peran di header; Muat Ulang hanya Admin/Analis.
- `PyroSnap/MainWindow.xaml.cs` — hapus riwayat navigasi setiap pindah halaman.
- `database/seed-local.example.sql` — templat akun uji tanpa password.

## 6. Rujukan
`PyroSnap/src/User.cs`, `PyroSnap/Views/LoginView.xaml.cs`, `database/001_schema.sql` (tabel `users`), `docs/database/acount-testing.md`, `Claude.md` §4.6, §5
