using Google.Apis.Auth.OAuth2;
using MVCMailing.Models;

namespace MVCMailing.Services;

public interface IEmailService
{
    Task<List<EmailMessageVm>> RetrieveEmails(GoogleCredential credential);
    Task<bool> SendEmail(GoogleCredential credential, SendModel? sendModel);
    EmailLoginVm loginCred { get; set; }
}