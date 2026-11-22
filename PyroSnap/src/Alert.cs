namespace PyroSnap;

// Diagram: Notification / Alert, dipicu oleh Hotspot
// Hotspot tidak memanggil Alert langsung; pemicunya dievaluasi setelah sinkronisasi
// (lihat Services/AmbangKritis.cs) lalu disimpan lewat AlertRepository.
public class Alert
{
    public string AlertId { get; set; } = "";
    public string Message { get; set; } = "";

    // internal set: diisi saat alert dibaca kembali dari database.
    public DateTime Timestamp { get; internal set; }
    public bool IsRead { get; internal set; }

    /// <summary>
    /// Siapkan alert untuk dikirim: catat waktu kirim (UTC) dan tandai belum dibaca.
    /// Penyimpanan ke tabel alert untuk tiap penerima dilakukan AlertRepository.
    /// </summary>
    public void SendAlert(string targetUser)
    {
        Timestamp = DateTime.UtcNow;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
