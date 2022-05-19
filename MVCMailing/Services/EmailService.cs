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
using Microsoft.IdentityModel.Tokens;


namespace MVCMailing.Services;

public class EmailService : IEmailService
{
    public EmailLoginVm loginCred { get; set; } = new();
    public async Task<List<EmailMessageVm>> RetrieveEmails(GoogleCredential credential)
    {
        // Create Gmail API service.
        var service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
        });

        var idsRequest = service.Users.Messages.List("me");
        var idList = (await idsRequest.ExecuteAsync()).Messages;

        List<EmailMessageVm> messages = new();
        for (int i = idList.Count - 1; i >= 0; i--)
        {
            var messageRequest = service.Users.Messages.Get("me", idList[i].Id);
            var message = (await messageRequest.ExecuteAsync());
            messages.Add(new EmailMessageVm(message));
        }

        return messages;
    }

    public async Task<bool> SendEmail(GoogleCredential  credential, SendModel? sendModel)
    {
        if (sendModel is null
            || string.IsNullOrEmpty(sendModel.To)
            || string.IsNullOrEmpty(sendModel.Body)
            || string.IsNullOrEmpty(sendModel.Subject))
        {
            return false;
        }
        
        // Create Gmail API service.
        var service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
        });

        var emailResponse = service.Users.GetProfile("me");
        string? email = (await emailResponse.ExecuteAsync()).EmailAddress;
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }
        
        string message = $"To: {sendModel.To}\r\nSubject: {sendModel.Subject}\r\nContent-Type: text/html;charset=utf-8\r\n\r\n{sendModel.Body}";
        var newMsg = new Message { Raw = Base64UrlEncode(message) };
        var response = await service.Users.Messages.Send(newMsg, "me").ExecuteAsync();
        
        return response != null;
    }
    
    private static string Base64UrlEncode(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        // Special "url-safe" base64 encode.
        return Convert.ToBase64String(inputBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}