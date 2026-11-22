namespace PyroSnap;

// ADD-10 / OPS-01 (di luar class diagram, disetujui 2026-11-22):
// hasil cek lapangan atas sebuah hotspot oleh Operator/Admin.
public sealed class Verifikasi
{
    public const string ApiAktif = "api_aktif";
    public const string SudahPadam = "sudah_padam";
    public const string BukanKebakaran = "bukan_kebakaran";

    public static readonly string[] SemuaStatus = { ApiAktif, SudahPadam, BukanKebakaran };

    public string Status { get; init; } = "";
    public string? Catatan { get; init; }
    public DateTime Waktu { get; init; }          // UTC
    public string? Pemeriksa { get; init; }       // username; null bila akun sudah dihapus

    public string LabelStatus => Label(Status);

    public static string Label(string status) => status switch
    {
        ApiAktif => "Api aktif",
        SudahPadam => "Sudah padam",
        BukanKebakaran => "Bukan kebakaran",
        _ => status
    };
}
