using EmailValidation;

namespace MVCMailing.Models;

public class EmailLoginVm
{
    public string Email { get; set;  } = string.Empty;
    public string Password { get; set;  } = string.Empty;
    public string ImapServer { get; set;  } = string.Empty;
    public int ImapPort { get; set; }
    
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }

    public bool Google { get; set;  } = false;

    public bool IsValid => !string.IsNullOrEmpty(Email) && EmailValidator.Validate(Email, false, true)
                           && (Google || (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(ImapServer)));
}
