using System.Text;
using App.UTIL.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace App.INFRA.Messaging.RabbitMq;

/// <summary>
/// RabbitMQ message publisher.
/// Publishes messages to queues/exchanges with automatic retry support.
/// </summary>
public sealed class RabbitMqPublisher : IEventPublisher
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(
        IRabbitMqService rabbitMqService,
        IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqPublisher> logger)
    {
        _rabbitMqService = rabbitMqService;
        _settings = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Publishes a message to the specified destination queue.
    /// </summary>
    /// <param name="destination">Queue name (routing key)</param>
    /// <param name="message">Message body (typically JSON)</param>
    /// <param name="headers">Optional message headers</param>
    /// <param name="ct">Cancellation token</param>
    public Task PublishAsync(
        string destination,
        string message,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(message);
        ct.ThrowIfCancellationRequested();

        var retryCount = 0;
        Exception? lastException = null;

        while (retryCount <= _settings.RetryCount)
        {
            try
            {
                PublishInternal(destination, message, headers);
                
                _logger.LogDebug(
                    "Message published to {Destination}, size: {Size} bytes",
                    destination,
                    message.Length);
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                lastException = ex;
                retryCount++;
                
                if (retryCount <= _settings.RetryCount)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to publish to {Destination}, retry {Retry}/{MaxRetry}",
                        destination,
                        retryCount,
                        _settings.RetryCount);
                    
                    Thread.Sleep(_settings.RetryDelayMs * retryCount); // Exponential backoff
                }
            }
        }

        _logger.LogError(
            lastException,
            "Failed to publish message to {Destination} after {RetryCount} retries",
            destination,
            _settings.RetryCount);
        
        throw new InvalidOperationException(
            $"Failed to publish message to {destination} after {_settings.RetryCount} retries",
            lastException);
    }

    private void PublishInternal(
        string destination,
        string message,
        IDictionary<string, string>? headers)
    {
        using var channel = _rabbitMqService.CreateChannel();
        
        // Declare queue (idempotent)
        channel.QueueDeclare(
            queue: destination,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        // Bind to exchange if configured
        if (!string.IsNullOrWhiteSpace(_settings.ExchangeName))
        {
            channel.QueueBind(
                queue: destination,
                exchange: _settings.ExchangeName,
                routingKey: destination);
        }

        // Create message properties
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.DeliveryMode = 2; // Persistent
        properties.ContentType = "application/json";
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        if (headers is { Count: > 0 })
        {
            properties.Headers = headers.ToDictionary(
                static kvp => kvp.Key,
                static kvp => (object?)kvp.Value);
        }

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(
            exchange: _settings.ExchangeName ?? string.Empty,
            routingKey: destination,
            basicProperties: properties,
            body: body);
    }
}

