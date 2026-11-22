using System.Windows;
using System.Windows.Controls;

namespace PyroSnap.Views
{
    /// <summary>
    /// FE-03 / SCR-A1: perpindahan halaman dan penjaga sesi/peran di satu tempat.
    /// </summary>
    public static class Navigasi
    {
        public static void Ke(DependencyObject dari, Page tujuan)
        {
            if (Window.GetWindow(dari) is MainWindow main) main.Navigate(tujuan);
        }

        /// <summary>Kembali ke Dashboard bila ada sesi, selain itu ke Login.</summary>
        public static void KeBeranda(DependencyObject dari) =>
            Ke(dari, AppSession.AdaSesi ? new DashboardView() : new LoginView());

        /// <summary>true bila ada sesi (login atau Tamu); selain itu pindah ke Login.</summary>
        public static bool PastikanSesi(Page halaman)
        {
            if (AppSession.AdaSesi) return true;
            Ke(halaman, new LoginView());
            return false;
        }

        /// <summary>
        /// SCR-A9: halaman yang dibatasi peran. true bila boleh; selain itu dialihkan
        /// ke Dashboard (ada sesi) atau Login (tanpa sesi).
        /// </summary>
        public static bool PastikanHak(Page halaman, bool boleh)
        {
            if (AppSession.AdaSesi && boleh) return true;
            KeBeranda(halaman);
            return false;
        }
    }
}
