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
using UniqueId = MailKit.UniqueId;


namespace MVCMailing.Services;

public class EmailService : IEmailService
{
    private readonly IGoogleAuthProvider _auth;
    public EmailLoginVm LoginCred { get; set; } = new();

    public EmailService(IGoogleAuthProvider auth)
    {
        _auth = auth;
    }

    public async Task<List<EmailMessageVm>> RetrieveEmails(int maxCount = 10)
    {
        using var emailClient = new ImapClient();

        if (LoginCred.Google)
        {
            var oauth2 = new SaslMechanismOAuth2(LoginCred.Email, await GetGmailToken());
            await emailClient.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
            await emailClient.AuthenticateAsync(oauth2);
        }
        else
        {
            await emailClient.ConnectAsync(LoginCred.ImapServer, LoginCred.ImapPort, SecureSocketOptions.SslOnConnect);
            await emailClient.AuthenticateAsync(LoginCred.Email, LoginCred.Password);
        }

        await emailClient.Inbox.OpenAsync(FolderAccess.ReadOnly);

        var itemCollection = await emailClient.Inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.BodyStructure);


        List<EmailMessageVm> emails = new();

        for (int i = itemCollection.Count - 1; i >= itemCollection.Count - 1 - maxCount; i--)
        {
            var message = await emailClient.Inbox.GetMessageAsync(i);
            var emailMessage = new EmailMessageVm
            {
                Body = string.IsNullOrEmpty(message.TextBody) ? string.Empty : message.TextBody,
                Subject = message.Subject,
                SenderName = message.From.Aggregate(string.Empty, (current, sender) => current + sender.Name),
                SenderEmail = string.Join("\n", message.From),
                Date = message.Date.ToString(),
                MessageUid = itemCollection[i].UniqueId.Id
            };
            emails.Add(emailMessage);
        }

        await emailClient.DisconnectAsync(true);
        return emails;
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
        else
        {
            await emailClient.ConnectAsync(LoginCred.SmtpServer, LoginCred.SmtpPort);
            await emailClient.AuthenticateAsync(LoginCred.Email, LoginCred.Password);
        }

        _ = await emailClient.SendAsync(email);
        await emailClient.DisconnectAsync(true);
        return true;
    }

    public async Task<bool> DeleteEmail(uint messageUid)
    {
        using var emailClient = new ImapClient();

        if (LoginCred.Google)
        {
            var oauth2 = new SaslMechanismOAuth2(LoginCred.Email, await GetGmailToken());
            await emailClient.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
            await emailClient.AuthenticateAsync(oauth2);
        }
        else
        {
            await emailClient.ConnectAsync(LoginCred.ImapServer, LoginCred.ImapPort, SecureSocketOptions.SslOnConnect);
            await emailClient.AuthenticateAsync(LoginCred.Email, LoginCred.Password);
        }

        await emailClient.Inbox.OpenAsync(FolderAccess.ReadWrite);
        try
        {
            var uid = new UniqueId(messageUid);
            await emailClient.Inbox.AddFlagsAsync(uid, MessageFlags.Deleted, true);
            await emailClient.Inbox.ExpungeAsync(new[]{uid});
        }
        catch
        {
            return false;
        }

        return true;
    }
    
    private async Task<string> GetGmailToken()
    {
        var googleCred = await _auth.GetCredentialAsync();
        return await googleCred.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}