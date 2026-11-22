using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Npgsql;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// Peringatan operasional milik user (FT-07c / ALR-06), hanya Admin & Operator (ROLE-03):
    /// belum dibaca di atas, bisa dibuka ke titik panasnya (UX-11) dan ditandai dibaca.
    /// </summary>
    public partial class AlertView : Page
    {
        public AlertView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (!Navigasi.PastikanHak(this, AppSession.TerimaAlertOperasional)) return;
                await MuatAsync();
            };
        }

        private async Task MuatAsync()
        {
            if (AppSession.UserIdAngka is not int userId) return;

            try
            {
                var daftar = await AlertRepository.DaftarDenganHotspotAsync(userId);
                GridAlert.ItemsSource = daftar.Select(d => new AlertRow(d.Alert, d.Hotspot)).ToList();

                int baru = daftar.Count(d => !d.Alert.IsRead);
                LblInfo.Text = daftar.Count == 0
                    ? "Belum ada peringatan."
                    : "Klik dua kali sebuah baris untuk membuka titik panasnya.";
                LblJumlahBaru.Text = baru.ToString("N0");
                LblJumlahTotal.Text = daftar.Count.ToString("N0");
                BtnTandai.IsEnabled = baru > 0;
                BtnTandaiSemua.IsEnabled = baru > 0;
                BtnBukaHotspot.IsEnabled = daftar.Count > 0;

                // Lencana lonceng di shell ikut diperbarui setelah dibaca/ditandai.
                if (Window.GetWindow(this) is MainWindow utama) _ = utama.PerbaruiLencanaAsync();
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                TampilkanGagal(ex);
            }
        }

        private AlertRow? BarisTerpilih()
        {
            if (GridAlert.SelectedItem is AlertRow row) return row;
            PesanUi.Tampilkan(BarPesan, LblPesan, "Pilih salah satu peringatan dulu.", JenisPesan.Peringatan);
            return null;
        }

        // UX-11: buka titik panas dari peringatan (sekaligus menandai dibaca).
        private void GridAlert_MouseDoubleClick(object sender, MouseButtonEventArgs e) => BukaHotspot();

        private void BtnBukaHotspot_Click(object sender, RoutedEventArgs e) => BukaHotspot();

        private async void BukaHotspot()
        {
            if (BarisTerpilih() is not { } row) return;
            if (row.Hotspot == null)
            {
                PesanUi.Tampilkan(BarPesan, LblPesan, "Titik panas untuk peringatan ini sudah tidak ada di database.",
                                  JenisPesan.Peringatan);
                return;
            }

            if (!row.Source.IsRead && AppSession.UserIdAngka is int userId)
            {
                try { await AlertRepository.TandaiDibacaAsync(row.Source, userId); }
                catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException) { /* tetap buka detail */ }
            }

            AppSession.SelectedHotspot = row.Hotspot;
            Navigasi.Ke(this, new DetailView(() => new AlertView()));
        }

        private async void BtnTandai_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.UserIdAngka is not int userId || BarisTerpilih() is not { } row) return;

            try
            {
                await AlertRepository.TandaiDibacaAsync(row.Source, userId);
                await MuatAsync();
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                TampilkanGagal(ex);
            }
        }

        private async void BtnTandaiSemua_Click(object sender, RoutedEventArgs e)
        {
            if (AppSession.UserIdAngka is not int userId) return;

            try
            {
                await AlertRepository.TandaiSemuaDibacaAsync(userId);
                await MuatAsync();
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                TampilkanGagal(ex);
            }
        }

        private void TampilkanGagal(Exception ex) =>
            PesanUi.Tampilkan(BarPesan, LblPesan, "Database tidak tersedia: " + PesanUi.Ringkas(ex.Message), JenisPesan.Error);

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Navigasi.KeBeranda(this);
    }

    /// <summary>Pembungkus tampilan satu peringatan untuk DataGrid.</summary>
    public class AlertRow
    {
        public Alert Source { get; }
        public Hotspot? Hotspot { get; }

        public AlertRow(Alert alert, Hotspot? hotspot)
        {
            Source = alert;
            Hotspot = hotspot;
        }

        public bool Baru => !Source.IsRead;
        public string Status => Source.IsRead ? "Dibaca" : "Baru";
        public string Waktu => Source.Timestamp.ToLocalTime().ToString("dd MMM yyyy HH:mm");
        public string Pesan => Source.Message;
    }
}
