using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly bool _emailEnabled;

    public SmtpEmailSender(IConfiguration config)
    {
        _config = config;
        _emailEnabled = bool.TryParse(config["EmailEnabled"], out var enabled) && enabled;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        if (!_emailEnabled)
        {
            Console.WriteLine($"[Email BLOCKED] To: {to}, Subject: {subject}");
            return;
        }

        var smtpSection = _config.GetSection("SmtpSettings");
        var fromAddress = smtpSection["From"];
        var portAddress = smtpSection["Port"];
        var enableSslSetting = smtpSection["EnableSsl"];

        if (string.IsNullOrWhiteSpace(fromAddress)) throw new InvalidOperationException("SMTP 'From' is not configured");
        if (string.IsNullOrWhiteSpace(portAddress)) throw new InvalidOperationException("SMTP 'Port' is not configured");
        if (string.IsNullOrWhiteSpace(enableSslSetting)) throw new InvalidOperationException("SMTP 'EnableSsl' is not configured");

        var message = new MailMessage
        {
            From = new MailAddress(fromAddress),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);

        using var client = new SmtpClient(smtpSection["Host"], int.Parse(portAddress))
        {
            Credentials = new NetworkCredential(smtpSection["UserName"], smtpSection["Password"]),
            EnableSsl = bool.Parse(enableSslSetting)
        };

        await client.SendMailAsync(message);
    }
}
