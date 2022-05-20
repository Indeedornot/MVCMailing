﻿using Google.Apis.Auth.OAuth2;
using MailKit;
using MVCMailing.Models;

namespace MVCMailing.Services;

public interface IEmailService
{
    Task<List<EmailMessageVm>> RetrieveEmails(int maxCount = 10);
    Task<bool> SendEmail(EmailMessageVm message);
    Task<bool> DeleteEmail(uint messageUID);
    EmailLoginVm LoginCred { get; set; }
}