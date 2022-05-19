namespace MVCMailing.Models;

public class EmailLoginVm
{
    public EmailLoginVm(string email, bool google)
    {
        Email = email;
        Google = google;
    }

    public EmailLoginVm()
    {
        //
    }

    public string Email { get; set;  } = string.Empty;
    public string Password { get; set;  } = string.Empty;
    public string ImapServer { get; set;  } = string.Empty;
    public bool Google { get; set;  } = false;
}
