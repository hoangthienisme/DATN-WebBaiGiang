using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WebBaiGiang.ViewModel;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        string senderName = emailSettings.GetValue<string>("SenderName");
        string senderEmail = emailSettings.GetValue<string>("SenderEmail");
        string smtpHost = emailSettings.GetValue<string>("SmtpHost");
        int smtpPort = emailSettings.GetValue<int>("SmtpPort");
        string senderPassword = emailSettings.GetValue<string>("SenderPassword");

        using var message = new MailMessage();
        message.From = new MailAddress(senderEmail, senderName);
        message.To.Add(new MailAddress(toEmail));
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = false;

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true,
        };

        // Gửi mail async
        await smtpClient.SendMailAsync(message);
    }
}
