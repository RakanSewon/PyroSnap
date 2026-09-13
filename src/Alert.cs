namespace PyroSnap;

// Diagram: Notification / Alert, dipicu oleh Hotspot
public class Alert
{
    public string AlertId { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; private set; }
    public bool IsRead { get; private set; }

    public void SendAlert(string targetUser)
    {
        Timestamp = DateTime.Now;
        IsRead = false;
        Console.WriteLine("[ALERT ke " + targetUser + "] " + Message);
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
