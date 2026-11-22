using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// SCR-06 / ROLE-04 (K10): warga mendaftar sendiri. Akun baru selalu berperan Publik,
    /// lalu langsung masuk ke Dashboard.
    /// </summary>
    public partial class RegisterView : Page
    {
        public RegisterView()
        {
            InitializeComponent();
            Loaded += (_, _) => TxtUsername.Focus();
        }

        private async void BtnDaftar_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;
            string konfirmasi = TxtKonfirmasi.Password;

            // Validasi di tempat sebelum menghubungi database.
            string? salah = UserRepository.ValidasiDaftar(username, password, konfirmasi);
            if (salah != null)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan, salah, JenisPesan.Peringatan);
                return;
            }

            BtnDaftar.IsEnabled = false;
            PesanUi.Tampilkan(BarPesan, LblPesan, "Membuat akun...", JenisPesan.Info);
            try
            {
                User user = await UserRepository.DaftarPublikAsync(username, password, konfirmasi);

                AppSession.Bersihkan();
                AppSession.CurrentUser = user;
                Navigasi.Ke(this, new DashboardView());
            }
            catch (ArgumentException ex)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan, ex.Message, JenisPesan.Peringatan);
            }
            catch (InvalidOperationException ex)
            {
                // Username sudah dipakai, migrasi 008 belum dijalankan, atau koneksi belum diatur.
                PesanUi.Tampilkan(BarPesan, LblPesan, ex.Message, JenisPesan.Peringatan);
            }
            catch (NpgsqlException ex)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan,
                    "Database tidak tersedia, akun belum bisa dibuat. Coba lagi beberapa saat lagi. " + PesanUi.Ringkas(ex.Message),
                    JenisPesan.Error);
            }
            finally
            {
                BtnDaftar.IsEnabled = true;
            }
        }

        private void BtnKembali_Click(object sender, RoutedEventArgs e) => Navigasi.Ke(this, new LoginView());
    }
}
