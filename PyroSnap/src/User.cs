namespace PyroSnap;

// Diagram: User / Account
// Atribut "password" pada diagram disimpan sebagai hash BCrypt (Claude.md §4.6),
// tidak pernah sebagai teks asli.
public class User
{
    private bool sudahLogin;

    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public string PasswordHash { private get; set; } = "";
    public string Role { get; set; } = "";

    public bool SudahLogin => sudahLogin;

    public bool Login(string username, string pass)
    {
        sudahLogin = false;
        if (string.IsNullOrEmpty(PasswordHash) || Username != username)
        {
            return false;
        }

        try
        {
            sudahLogin = BCrypt.Net.BCrypt.Verify(pass, PasswordHash);
        }
        catch (Exception ex) when (ex is BCrypt.Net.SaltParseException or ArgumentException)
        {
            // Hash di database rusak/bukan BCrypt: anggap gagal, jangan crash.
            sudahLogin = false;
        }

        return sudahLogin;
    }

    public void Logout()
    {
        sudahLogin = false;
    }
}
