using System;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;
using System.Windows.Controls;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// Tampilan Login (Lab 7.1 versi WPF).
    /// Memverifikasi username dan password lewat fungsi database masuk_akun.
    /// Setelah berhasil, user aktif disimpan di AppSession dan berpindah ke DashboardView.
    /// </summary>
    public partial class LoginView : Page
    {
        public LoginView()
        {
            InitializeComponent();
            BtnTema.Content = Tema.IsGelap ? Ikon.Matahari : Ikon.Bulan;
            Loaded += LoginView_Loaded;
        }

        // DBA-02: cek koneksi database tanpa memblokir UI; DB mati tidak membuat aplikasi crash.
        private async void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string hasil = await DatabaseConnection.TestConnectionAsync();
                LblDbStatus.Text = "Database: " + hasil.Split('\n')[0];
            }
            catch (Exception ex)
            {
                LblDbStatus.Text = "Database tidak tersedia: " + PesanUi.Ringkas(ex.Message);
                LblDbStatus.SetResourceReference(ForegroundProperty, "FireRed");
            }
        }

        // Login lewat tabel users (hash BCrypt). Tidak ada akun bawaan di kode.
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

            if (username.Length == 0 || password.Length == 0)
            {
                TampilkanError("Username dan password wajib diisi.");
                return;
            }

            BtnLogin.IsEnabled = false;
            PesanUi.Sembunyikan(BarPesan);

            try
            {
                // Password diverifikasi di database; hash tidak pernah dikirim ke aplikasi.
                User? user = await UserRepository.MasukAsync(username, password);

                if (user == null)
                {
                    // Pesan sama untuk username tak dikenal dan password salah (ACC-03/04).
                    TampilkanError("Username atau password salah.");
                    TxtPassword.Clear();
                    return;
                }

                AppSession.Bersihkan();
                AppSession.CurrentUser = user;
                Navigasi.Ke(this, new DashboardView());
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                // AUTH-08: kegagalan database bukan "password salah".
                TampilkanError("Database tidak tersedia, login tidak dapat diperiksa. Coba lagi beberapa saat lagi. " +
                               PesanUi.Ringkas(ex.Message));
            }
            finally
            {
                BtnLogin.IsEnabled = true;
            }
        }

        // ---- Jalur warga (K10) ----
        private void BtnDaftar_Click(object sender, RoutedEventArgs e) => Navigasi.Ke(this, new RegisterView());

        private void BtnTamu_Click(object sender, RoutedEventArgs e)
        {
            AppSession.MasukSebagaiTamu();
            Navigasi.Ke(this, new DashboardView());
        }

        // FK5: Panduan bisa dibuka tanpa login.
        private void BtnPanduan_Click(object sender, RoutedEventArgs e) => Navigasi.Ke(this, new PanduanView());

        private void TampilkanError(string pesan)
        {
            PesanUi.Tampilkan(BarPesan, LblPesan, pesan, JenisPesan.Error);
        }

        // DS-06: ganti tema lalu muat ulang halaman ini.
        private void BtnTema_Click(object sender, RoutedEventArgs e)
        {
            Tema.Ganti();
            Navigasi.Ke(this, new LoginView());
        }
    }
}
