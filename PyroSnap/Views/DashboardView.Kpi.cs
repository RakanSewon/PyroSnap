using System.Linq;
using System.Windows;
using PyroSnap.Services;

namespace PyroSnap.Views
{
    /// <summary>
    /// Kartu KPI di atas peta (gaya D5). Semua angka dihitung dari titik yang sedang tampil
    /// (_tampil, setelah filter dan area), jadi selalu sejalan dengan peta dan daftar.
    /// </summary>
    public partial class DashboardView
    {
        private void PerbaruiKpi()
        {
            int total = _tampil.Count;
            LblKpiTotal.Text = total.ToString("N0");
            LblKpiTotalSub.Text = _areaFokus == null
                ? $"dari {_semua.Count:N0} titik data"
                : $"area {_areaFokus.NamaLokasi} · dari {_semua.Count:N0}";

            int tinggi = _tampil.Count(h => TingkatHotspot.Dari(h.Confidence) == Tingkat.Tinggi);
            LblKpiTinggi.Text = tinggi.ToString("N0");
            PilKpiTinggi.Visibility = total > 0 ? Visibility.Visible : Visibility.Collapsed;
            LblKpiTinggiPersen.Text = total > 0 ? $"{tinggi * 100.0 / total:0}%" : "";

            var perArea = _tampil
                .Select(h => WilayahPantau.Temukan(h)?.NamaLokasi)
                .Where(n => n != null)
                .GroupBy(n => n!)
                .OrderByDescending(g => g.Count())
                .ToList();
            LblKpiWilayah.Text = perArea.Count.ToString();
            LblKpiWilayahDari.Text = $"dari {WilayahPantau.Semua.Count}";
            LblKpiWilayahSub.Text = perArea.Count > 0
                ? $"Terbanyak: {perArea[0].Key} ({perArea[0].Count()})"
                : "Tidak ada titik di area pantau";

            var terkuat = _tampil.OrderByDescending(h => h.Frp).FirstOrDefault();
            LblKpiFrp.Text = terkuat != null ? terkuat.Frp.ToString("0.#") : "-";
            LblKpiFrpSub.Text = terkuat == null
                ? "Tidak ada titik"
                : WilayahPantau.Temukan(terkuat) is { } area
                    ? $"Sekitar {area.NamaLokasi}"
                    : "Di luar area pantau";
        }
    }
}
