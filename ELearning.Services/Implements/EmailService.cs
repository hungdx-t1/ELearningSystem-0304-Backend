using ELearning.Core.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace ELearning.Services.Implements;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();

        // Cấu hình người gửi dựa vào AppSettings
        var senderName = config["SmtpConfig:SenderName"] ?? "LMS Admin";
        var senderEmail = config["SmtpConfig:SenderEmail"];
        var password = config["SmtpConfig:Password"];
        var host = config["SmtpConfig:Host"] ?? "smtp.gmail.com";
        var portStr = config["SmtpConfig:Port"] ?? "587";

        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
            throw new Exception("SmtpConfig is not properly configured in appsettings.json");

        email.From.Add(new MailboxAddress(senderName, senderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        bool parsed = int.TryParse(portStr, out int port);
        if (!parsed) port = 587;

        await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(senderEmail, password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
