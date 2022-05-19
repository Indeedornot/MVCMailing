namespace MVCMailing.Models;

public class EmailLoginVm
{
    public string Email { get; set;  } = string.Empty;
    public string Password { get; set;  } = string.Empty;
    public string ImapServer { get; set;  } = string.Empty;
    public string SmptServer { get; set; } = string.Empty;
    public bool Google { get; set;  } = false;

    public bool IsValid => !string.IsNullOrEmpty(Email) 
                           && (Google || (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(ImapServer)));
}
