using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using PyroSnap.Services;
using PyroSnap.Views;

namespace PyroSnap
{
    /// <summary>
    /// Shell aplikasi: menu samping, bilah atas (judul, lonceng, profil), dan Frame halaman.
    /// Memenuhi syarat Modul 7: "pastikan perpindahan antar form sudah bisa dilakukan".
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Setiap pindah halaman: hapus riwayat Frame (Back/Alt+← tidak membuka halaman lama)
            // lalu sesuaikan shell dengan halaman & peran.
            MainFrame.Navigated += (_, e) =>
            {
                while (MainFrame.CanGoBack) MainFrame.RemoveBackEntry();
                AturShell(e.Content);
            };

            // Tampilan pertama: Masuk.
            Navigate(new LoginView());
        }

        /// <summary>Berpindah ke sebuah halaman. Dipanggil lewat Views/Navigasi.cs.</summary>
        public void Navigate(Page page) => MainFrame.Navigate(page);

        // ---- Shell per halaman ----
        private void AturShell(object halaman)
        {
            bool pakaiShell = AppSession.AdaSesi && halaman is not (LoginView or RegisterView);
            Sidebar.Visibility = pakaiShell ? Visibility.Visible : Visibility.Collapsed;
            BilahAtas.Visibility = pakaiShell ? Visibility.Visible : Visibility.Collapsed;
            KolomSidebar.Width = pakaiShell ? new GridLength(240) : new GridLength(0);
            AreaKanan.Margin = pakaiShell ? new Thickness(16) : new Thickness(0);

            BtnTema.Content = Tema.IsGelap ? "Tema terang" : "Tema gelap";
            BtnTema.Tag = Tema.IsGelap ? Ikon.Matahari : Ikon.Bulan;
            if (!pakaiShell) return;

            // Judul halaman di bilah atas
            (TxtJudul.Text, TxtSubjudul.Text) = halaman switch
            {
                DashboardView => ("Dashboard", "Titik panas Indonesia dari satelit NASA FIRMS"),
                DetailView => ("Detail Titik Panas", "Data satelit, kondisi, dan hasil cek lapangan"),
                AlertView => ("Peringatan Operasional", "Titik berkeyakinan tinggi (≥ 80%, kelas LAPAN: segera penanggulangan)"),
                ExportView => ("Ekspor Laporan", "Simpan titik yang sedang tampil sebagai TXT atau CSV"),
                PanduanView => ("Panduan & Kontak Darurat", "Apa yang dilakukan saat ada asap atau kebakaran"),
                _ => ("PyroSnap", "")
            };

            // Menu sesuai peran (role-based-ui.md) dan menu aktif
            NavPeringatan.Visibility = AppSession.TerimaAlertOperasional ? Visibility.Visible : Visibility.Collapsed;
            NavEkspor.Visibility = AppSession.BolehEkspor ? Visibility.Visible : Visibility.Collapsed;
            WadahLonceng.Visibility = AppSession.TerimaAlertOperasional ? Visibility.Visible : Visibility.Collapsed;
            NavDashboard.IsChecked = halaman is DashboardView or DetailView;
            NavPeringatan.IsChecked = halaman is AlertView;
            NavEkspor.IsChecked = halaman is ExportView;
            NavPanduan.IsChecked = halaman is PanduanView;

            // Profil
            string nama = AppSession.IsTamu ? "Tamu" : AppSession.CurrentUser?.Username ?? "";
            TxtNamaUser.Text = nama;
            TxtPeranUser.Text = AppSession.IsTamu ? "Tanpa akun" : AppSession.Peran;
            TxtInisial.Text = new string(nama.Where(char.IsLetterOrDigit).Take(2).ToArray()).ToUpperInvariant();
            BtnKeluar.Content = AppSession.IsTamu ? "Keluar (Tamu)" : "Keluar";

            _ = PerbaruiLencanaAsync();
        }

        /// <summary>Jumlah peringatan belum dibaca di lonceng (Admin & Operator).</summary>
        public async Task PerbaruiLencanaAsync()
        {
            if (!AppSession.TerimaAlertOperasional || AppSession.UserIdAngka is not int userId)
            {
                Lencana.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                int belumDibaca = await AlertRepository.JumlahBelumDibacaAsync(userId);
                TxtLencana.Text = belumDibaca > 99 ? "99+" : belumDibaca.ToString();
                Lencana.Visibility = belumDibaca > 0 ? Visibility.Visible : Visibility.Collapsed;
                BtnLonceng.ToolTip = belumDibaca > 0 ? $"{belumDibaca} peringatan belum dibaca" : "Tidak ada peringatan baru";
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                Lencana.Visibility = Visibility.Collapsed;
                BtnLonceng.ToolTip = "Jumlah peringatan tidak bisa dibaca: database tidak tersedia.";
            }
        }

        // ---- Menu ----
        private void NavDashboard_Click(object sender, RoutedEventArgs e) => Navigate(new DashboardView());

        private void NavPeringatan_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.TerimaAlertOperasional) Navigate(new AlertView());
        }

        private void NavEkspor_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.BolehEkspor) Navigate(new ExportView());
        }

        private void NavPanduan_Click(object sender, RoutedEventArgs e) => Navigate(new PanduanView());

        private void BtnKeluar_Click(object sender, RoutedEventArgs e)
        {
            AppSession.Bersihkan();
            Navigate(new LoginView());
        }

        /// <summary>
        /// DS-06: ganti tema lalu muat ulang halaman aktif. Warna lewat DynamicResource ikut
        /// berubah sendiri; muat ulang memperbarui warna yang disetel dari kode dan peta.
        /// State halaman tersimpan di AppSession, jadi data & filter tidak hilang.
        /// </summary>
        private void BtnTema_Click(object sender, RoutedEventArgs e)
        {
            Tema.Ganti();
            Page? baru = MainFrame.Content switch
            {
                DashboardView => new DashboardView(),
                DetailView detail => detail.Salinan(),
                AlertView => new AlertView(),
                ExportView => new ExportView(),
                PanduanView => new PanduanView(),
                _ => null
            };
            if (baru != null) Navigate(baru);
            else AturShell(MainFrame.Content);
        }
    }
}
