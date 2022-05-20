using EmailValidation;
using Google.Apis.Gmail.v1.Data;
using MailKit;
using MailKit.Net.Smtp;

namespace MVCMailing.Models;

public class EmailMessageVm
{
    public string Date { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string To { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public uint MessageUid  { get; set; }

    public bool IsValidSend =>
        !string.IsNullOrEmpty(To) && EmailValidator.Validate(To, false, true) && !string.IsNullOrEmpty(Subject) && !string.IsNullOrEmpty(Body);
}