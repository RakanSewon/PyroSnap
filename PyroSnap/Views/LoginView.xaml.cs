using System.Windows;
using System.Windows.Controls;

namespace PyroSnap.Views
{
    /// <summary>
    /// Tampilan Login (Lab 7.1 versi WPF).
    /// Memakai model User milik tim untuk verifikasi kredensial.
    /// Setelah berhasil, berpindah ke DashboardView.
    /// </summary>
    public partial class LoginView : Page
    {
        // Akun demo untuk keperluan tampilan praktikum.
        private const string DemoUser = "admin";
        private const string DemoPass = "pyrosnap";

        public LoginView()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

            // Memakai model User (src/User.cs) yang sudah dibuat tim.
            var user = new User
            {
                Username = DemoUser,
                Password = DemoPass,
                Role = "User"
            };

            if (user.Login(username, password) || (username == DemoUser && password == DemoPass))
            {
                var main = (MainWindow)Window.GetWindow(this);
                main.Navigate(new DashboardView());
            }
            else
            {
                LblError.Text = "Username atau password salah.";
                LblError.Visibility = Visibility.Visible;
            }
        }
    }
}
