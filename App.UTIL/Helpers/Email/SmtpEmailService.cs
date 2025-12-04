using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App.UTIL.Helpers.Email;

public class SmtpEmailService : IEmailService
{
    private readonly string? _fromEmail;
    private readonly string _fromName;
    private readonly string? _smtpHost;
    private readonly int _smtpPort;
    private readonly string? _smtpUser;
    private readonly string? _smtpPassword;
    private readonly bool _enableSsl;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _logger = logger;

        _fromEmail = configuration["EmailSettings:FromEmail"];
        _fromName = configuration["EmailSettings:FromName"] ?? "Ecom Website";
        _smtpHost = configuration["EmailSettings:SmtpHost"];
        _smtpUser = configuration["EmailSettings:SmtpUser"];
        _smtpPassword = configuration["EmailSettings:SmtpPassword"];
        _enableSsl = configuration.GetValue("EmailSettings:EnableSsl", true);
        _smtpPort = configuration.GetValue("EmailSettings:SmtpPort", 587);
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(htmlBody);

        if (string.IsNullOrWhiteSpace(_smtpHost))
        {
            _logger.LogWarning("Skipping email send because SMTP host is not configured. Subject: {Subject}", subject);
            return;
        }

        using var message = new MailMessage
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        var fromAddress = ResolveFromAddress();
        message.From = fromAddress;
        message.To.Add(new MailAddress(to));

        using var client = CreateClient();

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient} with subject {Subject}", to, subject);
            throw;
        }
    }

    private MailAddress ResolveFromAddress()
    {
        var address = !string.IsNullOrWhiteSpace(_fromEmail)
            ? _fromEmail
            : _smtpUser;

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidOperationException("Email settings are missing FromEmail/SmtpUser configuration.");
        }

        return new MailAddress(address, _fromName ?? string.Empty);
    }

    private SmtpClient CreateClient()
    {
        var client = new SmtpClient
        {
            Host = _smtpHost!,
            Port = _smtpPort,
            EnableSsl = _enableSsl
        };

        if (!string.IsNullOrWhiteSpace(_smtpUser))
        {
            client.Credentials = new NetworkCredential(_smtpUser, _smtpPassword);
        }

        return client;
    }
}

