using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App.INFRA.Email;

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

        // Đọc từ IConfiguration (đã được merge với env variables tự động)
        var emailSection = configuration.GetSection("EmailSettings");
        
        _fromEmail = emailSection["FromEmail"];
        if (!string.IsNullOrWhiteSpace(_fromEmail) && _fromEmail.Contains("${", StringComparison.Ordinal))
        {
            _fromEmail = null;
        }
        
        var fromName = emailSection["FromName"];
        if (!string.IsNullOrWhiteSpace(fromName) && fromName.Contains("${", StringComparison.Ordinal))
        {
            fromName = null;
        }
        _fromName = fromName ?? "Ecom Website";
        
        _smtpHost = emailSection["SmtpHost"];
        if (!string.IsNullOrWhiteSpace(_smtpHost) && _smtpHost.Contains("${", StringComparison.Ordinal))
        {
            _smtpHost = null;
        }
        
        _smtpUser = emailSection["SmtpUser"];
        if (!string.IsNullOrWhiteSpace(_smtpUser) && _smtpUser.Contains("${", StringComparison.Ordinal))
        {
            _smtpUser = null;
        }
        
        _smtpPassword = emailSection["SmtpPassword"];
        if (!string.IsNullOrWhiteSpace(_smtpPassword) && _smtpPassword.Contains("${", StringComparison.Ordinal))
        {
            _smtpPassword = null;
        }
        
        var enableSslStr = emailSection["EnableSsl"];
        if (!string.IsNullOrWhiteSpace(enableSslStr) && enableSslStr.Contains("${", StringComparison.Ordinal))
        {
            enableSslStr = null;
        }
        _enableSsl = !string.IsNullOrWhiteSpace(enableSslStr) && bool.TryParse(enableSslStr, out var parsed) && parsed
            || emailSection.GetValue("EnableSsl", true);
        
        var smtpPortStr = emailSection["SmtpPort"];
        if (!string.IsNullOrWhiteSpace(smtpPortStr) && smtpPortStr.Contains("${", StringComparison.Ordinal))
        {
            smtpPortStr = null;
        }
        _smtpPort = !string.IsNullOrWhiteSpace(smtpPortStr) && int.TryParse(smtpPortStr, out var parsedPort) 
            ? parsedPort 
            : emailSection.GetValue("SmtpPort", 587);
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

