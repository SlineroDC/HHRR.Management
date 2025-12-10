using System.Net;
using System.Net.Mail;
using HHRR.Application.Interfaces;

namespace HHRR.Infrastructure.Services;

public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
        var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
        var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
        var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            Console.WriteLine("[WARNING] SMTP Configuration missing. Email not sent.");
            return;
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUser, "HHRR Management"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        try
        {
            await client.SendMailAsync(mailMessage);
            Console.WriteLine($"[INFO] Email sent to {to}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to send email: {ex.Message}");
        }
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string name)
    {
        var subject = "Welcome to HHRR Management";
        var body = $@"
            <h1>Welcome, {name}!</h1>
            <p>We are excited to have you on board.</p>
            <p>Your account has been successfully created.</p>
            <br>
            <p>Best regards,</p>
            <p>HHRR Team</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }
}
