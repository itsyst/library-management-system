using DnsClient;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Library.Infrastructure.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    private readonly IConfiguration _config = config;

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_config["Email:FromEmail"]));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = body };

        using var smtp = new SmtpClient();
        var smtpServer = _config["Email:SmtpServer"] ?? "smtp.gmail.com";
        var port = int.Parse(_config["Email:Port"] ?? "587");
        await smtp.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }

    public async Task<bool> EmailVerifier(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return false;

        var domain = email.Split('@').Last();

        var lookup = new LookupClient();
        var result = await lookup.QueryAsync(domain, QueryType.MX);

        var mxRecords = result.Answers.MxRecords().ToList();

        return mxRecords.Count != 0;
    }
}


