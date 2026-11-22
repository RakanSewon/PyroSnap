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
    // Bagian Dashboard: cari lokasi dan filter.
    public partial class DashboardView
    {
        // ---- Kriteria (filter + area) diterapkan dari data penuh ----
        private void TerapkanKriteria(Hotspot? pilih = null)
        {
            IEnumerable<Hotspot> hasil = _semua.Where(_filter.IsMatch);
            if (_areaFokus != null)
                hasil = hasil.Where(h => WilayahPantau.DalamArea(h, _areaFokus));

            _tampil = hasil.ToList();

            AppSession.Filter = _filter;
            AppSession.AreaFokus = _areaFokus;
            AppSession.TeksCari = _areaFokus != null ? _areaFokus.NamaLokasi : "";

            RefreshTampilan(pilih);
            PerbaruiRingkasan();
        }

        private void RefreshTampilan(Hotspot? pilih = null)
        {
            GridHotspot.ItemsSource = _tampil.Select(h => new HotspotRow(h)).ToList();
            LblCount.Text = _areaFokus == null
                ? $"Menampilkan {_tampil.Count} dari {_semua.Count} titik"
                : $"Menampilkan {_tampil.Count} dari {_semua.Count} titik · area {_areaFokus.NamaLokasi}";
            LblTabDaftar.Text = $"Daftar Titik Panas ({_tampil.Count})";
            AppSession.CurrentHotspots = _tampil;

            if (_useWeb) KirimDataKePeta();
            else GambarPeta();

            var baris = pilih != null ? CariBaris(pilih) : null;
            if (baris != null)
            {
                GridHotspot.SelectedItem = baris;
                GridHotspot.ScrollIntoView(baris);
            }
            else if (GridHotspot.Items.Count > 0)
            {
                GridHotspot.SelectedIndex = 0;
            }
        }

        private HotspotRow? CariBaris(Hotspot hs) =>
            (GridHotspot.ItemsSource as IEnumerable<HotspotRow>)?
                .FirstOrDefault(r => ReferenceEquals(r.Source, hs));

        private void IsiPilihanWilayah()
        {
            var nama = WilayahPantau.Semua.Select(l => l.NamaLokasi).ToList();
            CmbLokasi.ItemsSource = nama;
            CmbWilayahSaya.ItemsSource = nama;
        }

        // ---- Use case: Select / Search Location ----
        private void BtnSearch_Click(object sender, RoutedEventArgs e) => CariLokasi();

        private void CmbLokasi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) CariLokasi();
        }

        private void CariLokasi()
        {
            string kata = CmbLokasi.Text.Trim();
            if (string.IsNullOrEmpty(kata))
            {
                _areaFokus = null;
                TerapkanKriteria();
                return;
            }

            var lokasi = WilayahPantau.Cari(kata);
            if (lokasi == null)
            {
                TampilkanStatus($"Lokasi \"{kata}\" tidak ada di daftar area pantau.",
                    "Pilih dari daftar: " + string.Join(", ", WilayahPantau.Semua.Select(l => l.NamaLokasi)) + ".",
                    JenisPesan.Peringatan);
                return;
            }

            _areaFokus = lokasi;
            CmbLokasi.Text = lokasi.NamaLokasi;
            TerapkanKriteria();
            FokuskanPeta(lokasi);

            if (_tampil.Count == 0)
                TampilkanStatus($"Tidak ada titik panas di sekitar {lokasi.NamaLokasi} yang sesuai filter.",
                                "Coba longgarkan filter atau tekan Reset.", JenisPesan.Info);
        }

        // ---- Use case: Filter Hotspots ----
        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            _filter.ConfidenceMin = (int)SldConfidence.Value;
            _filter.FrpMin = SldFrp.Value;
            TerapkanKriteria();

            if (_tampil.Count == 0 && _semua.Count > 0)
                TampilkanStatus("Tidak ada titik panas yang sesuai filter.", "Coba longgarkan filter atau tekan Reset.",
                                JenisPesan.Info);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _filter.Reset();
            SldConfidence.Value = 0;
            SldFrp.Value = 0;
            CmbLokasi.Text = "";
            _areaFokus = null;
            _fitPeta = true;
            TerapkanKriteria();
        }

        private void SldConfidence_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LblConfidence != null)
                LblConfidence.Text = $"{(int)e.NewValue} % ({TingkatHotspot.Nama(TingkatHotspot.Dari((int)e.NewValue))} ke atas)";
        }

        private void SldFrp_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (LblFrp != null) LblFrp.Text = $"{(int)e.NewValue} MW";
        }
    }
}
