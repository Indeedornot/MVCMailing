using System.Buffers.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MVCMailing.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.AspNetCore3;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using MimeKit.Text;


namespace MVCMailing.Services;

public class EmailService : IEmailService
{
    private readonly IGoogleAuthProvider _auth;
    public EmailLoginVm LoginCred { get; set; } = new();

    public EmailService(IGoogleAuthProvider auth)
    {
        _auth = auth;
    }

    // public async Task<List<EmailMessageVm>> RetrieveEmails(GoogleCredential credential)
    // {
    //     // Create Gmail API service.
    //     var service = new GmailService(new BaseClientService.Initializer
    //     {
    //         HttpClientInitializer = credential,
    //     });
    //
    //     var idsRequest = service.Users.Messages.List("me");
    //     var idList = (await idsRequest.ExecuteAsync()).Messages;
    //
    //     List<EmailMessageVm> messages = new();
    //     for (int i = idList.Count - 1; i >= 0; i--)
    //     {
    //         var messageRequest = service.Users.Messages.Get("me", idList[i].Id);
    //         var message = (await messageRequest.ExecuteAsync());
    //         messages.Add(new EmailMessageVm(message));
    //     }
    //
    //     return messages;
    // }


    public async Task<List<EmailMessageVm>> RetrieveEmails(int maxCount = 10)
    {
        using var emailClient = new ImapClient();

        if (LoginCred.Google)
        {
            var oauth2 = new SaslMechanismOAuth2(LoginCred.Email, await GetGmailToken());
            await emailClient.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
            await emailClient.AuthenticateAsync(oauth2);
        }
        else //TODO ADD GET SERVER FROM EMAIL
        {
            await emailClient.ConnectAsync(LoginCred.ImapServer, LoginCred.ImapPort, SecureSocketOptions.SslOnConnect);
            await emailClient.AuthenticateAsync(LoginCred.Email, LoginCred.Password);
        }

        await emailClient.Inbox.OpenAsync(FolderAccess.ReadOnly);

        //var itemCollection = emailClient.Inbox.Fetch(uids, MessageSummaryItems.UniqueId | MessageSummaryItems.BodyStructure);


        List<EmailMessageVm> emails = new();
        for (int i = emailClient.Inbox.Count - 1; i >= emailClient.Inbox.Count - 1 - maxCount; i--)
        {
            var message = await emailClient.Inbox.GetMessageAsync(i);
            var emailMessage =
                new EmailMessageVm
                {
                    Body = !string.IsNullOrEmpty(message.TextBody) ? string.Empty : message.TextBody,
                    Subject = message.Subject,
                    SenderName = message.From.Aggregate(string.Empty, (current, sender) => current + sender.Name),
                    SenderEmail = string.Join("\n", message.From),
                    Date = message.Date.ToString()
                };
            emails.Add(emailMessage);
        }

        await emailClient.DisconnectAsync(true);
        return emails;
    }

    [GoogleScopedAuthorize(GmailService.ScopeConstants.MailGoogleCom)]
    public async Task<string> GetGmailToken()
    {
        var googleCred = await _auth.GetCredentialAsync();
        return await googleCred.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }

    public async Task<bool> SendEmail(EmailMessageVm message)
    {
        string nameTo = message.To.Split("@")[0];
        string nameFrom = LoginCred.Email.Split("@")[0];
        
        var email = new MimeMessage
        {
            Subject = message.Subject,
            Body = new TextPart("plain"){ Text = message.Body },
            
        };
        email.From.Add(new MailboxAddress(nameFrom, LoginCred.Email));
        email.To.Add(new MailboxAddress(nameTo, message.To));
        
        // send email
        using var emailClient = new SmtpClient();

        if (LoginCred.Google)
        {
            var oauth2 = new SaslMechanismOAuth2(LoginCred.Email, await GetGmailToken());
            await emailClient.ConnectAsync ("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
            await emailClient.AuthenticateAsync(oauth2);
        }
        else //TODO ADD GET SERVER FROM EMAIL AND CORRECT PORT
        {
            await emailClient.ConnectAsync(LoginCred.SmtpServer, LoginCred.SmtpPort, SecureSocketOptions.Auto);
            await emailClient.AuthenticateAsync(LoginCred.Email, LoginCred.Password);
        }

        string? response = await emailClient.SendAsync(email);
        await emailClient.DisconnectAsync(true);
        return true;
    }
}