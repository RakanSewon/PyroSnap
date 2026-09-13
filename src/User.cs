namespace PyroSnap;

// Diagram: User / Account
public class User
{
    private bool sudahLogin;

    public string UserId { get; set; }
    public string Username { get; set; }
    public string Password { private get; set; }
    public string Role { get; set; }

    public bool Login(string username, string pass)
    {
        sudahLogin = Username == username && Password == pass;
        return sudahLogin;
    }

    public void Logout()
    {
        sudahLogin = false;
    }
}
