using Google.Apis.Gmail.v1.Data;

namespace MVCMailing.Models;

public class EmailMessageVm
{
    public EmailMessageVm(Message message)
    {
        Snippet = message.Snippet;
        var headers = message.Payload.Headers.ToList();
        To = headers.Where(x => x.Name == "To").ToList()[0].Value;
        Sender = headers.Where(x => x.Name == "From").ToList()[0].Value;
        Date = headers.Where(x => x.Name == "Date").ToList()[0].Value;
        Subject = headers.Where(x => x.Name == "Subject").ToList()[0].Value;
    }
    
    public string Snippet { get; set; }
    public string To { get; set; }
    public string Sender { get; set; }
    public string Date { get; set; }
    public string Subject { get; set; }
}