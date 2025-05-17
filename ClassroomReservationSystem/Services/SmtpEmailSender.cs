using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    public SmtpEmailSender(IConfiguration config) => _config = config;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpSection = _config.GetSection("SmtpSettings");

        var fromAddress = smtpSection["From"];
        var PortAddress = smtpSection["Port"];
        var enableSslSetting = smtpSection["EnableSsl"];

        if (string.IsNullOrWhiteSpace(fromAddress))
            throw new InvalidOperationException("SMTP 'From' address is not configured");
        if (string.IsNullOrWhiteSpace(PortAddress))
            throw new InvalidOperationException("SMTP 'From' address is not configured");
        if (string.IsNullOrWhiteSpace(enableSslSetting))
            throw new InvalidOperationException("SMTP EnableSsl config missing");


        var message = new MailMessage
        {
            From = new MailAddress(fromAddress),
            Subject = subject,
            Body = body
        };

        message.To.Add(to);

        var client = new SmtpClient(smtpSection["Host"], int.Parse(PortAddress))
        {
            Credentials = new NetworkCredential(smtpSection["UserName"], smtpSection["Password"]),
            EnableSsl = bool.Parse(enableSslSetting)
        };

        await client.SendMailAsync(message);
    }
}
