using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace App.UTIL.Helpers.Email;

public sealed class QueueEmailService : IEmailService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;
    private readonly ILogger<QueueEmailService> _logger;
    private bool _disposed;

    public QueueEmailService(IConfiguration configuration, ILogger<QueueEmailService> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _logger = logger;

        IConfigurationSection mqSection = configuration.GetSection("RabbitMqSettings");
        var hostName = mqSection["HostName"] ?? "localhost";
        var port = mqSection.GetValue("Port", 5672);
        var userName = mqSection["UserName"] ?? "guest";
        var password = mqSection["Password"] ?? "guest";
        var virtualHost = mqSection["VirtualHost"] ?? "/";

        IConfigurationSection emailQueueSection = configuration.GetSection("EmailSettings:Queue");
        _exchangeName = emailQueueSection["ExchangeName"] ?? "email.exchange";
        var exchangeType = emailQueueSection["ExchangeType"] ?? "fanout";

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password,
            VirtualHost = virtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_exchangeName, exchangeType, durable: true, autoDelete: false);
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(htmlBody);

        var payload = new EmailMessage
        {
            To = to,
            Subject = subject,
            Body = htmlBody,
            ContentType = "text/html"
        };

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        try
        {
            _channel.BasicPublish(exchange: _exchangeName,
                                  routingKey: string.Empty,
                                  basicProperties: properties,
                                  body: bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish email message to exchange {Exchange}", _exchangeName);
            throw;
        }

        return Task.CompletedTask;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(QueueEmailService));
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _channel.Close();
            _connection.Close();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while disposing QueueEmailService");
        }
        finally
        {
            _channel.Dispose();
            _connection.Dispose();
            _disposed = true;
        }
    }

    private sealed record EmailMessage
    {
        public string To { get; init; } = string.Empty;
        public string Subject { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
        public string ContentType { get; init; } = "text/html";
    }
}

