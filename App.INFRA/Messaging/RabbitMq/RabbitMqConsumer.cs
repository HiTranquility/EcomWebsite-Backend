using System.Text;
using App.UTIL.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace App.INFRA.Messaging.RabbitMq;

/// <summary>
/// RabbitMQ message consumer.
/// Subscribes to queues and processes messages asynchronously.
/// </summary>
public sealed class RabbitMqConsumer : IEventConsumer
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(
        IRabbitMqService rabbitMqService,
        IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqConsumer> logger)
    {
        _rabbitMqService = rabbitMqService;
        _settings = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Subscribes to a queue and processes messages with the provided handler.
    /// </summary>
    /// <param name="destination">Queue name to subscribe to</param>
    /// <param name="handler">Message handler function</param>
    /// <param name="ct">Cancellation token for graceful shutdown</param>
    public Task SubscribeAsync(
        string destination,
        Func<string, IDictionary<string, string>?, Task> handler,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(handler);
        ct.ThrowIfCancellationRequested();

        var channel = _rabbitMqService.CreateChannel();
        
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

        var consumer = new AsyncEventingBasicConsumer(channel);
        
        consumer.Received += async (_, ea) =>
        {
            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            IDictionary<string, string>? headerValues = null;

            // Extract headers
            if (ea.BasicProperties?.Headers is { Count: > 0 } rawHeaders)
            {
                headerValues = new Dictionary<string, string>(rawHeaders.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var (key, value) in rawHeaders)
                {
                    headerValues[key] = value switch
                    {
                        byte[] bytes => Encoding.UTF8.GetString(bytes),
                        string str => str,
                        null => string.Empty,
                        _ => value.ToString() ?? string.Empty
                    };
                }
            }

            try
            {
                await handler(payload, headerValues).ConfigureAwait(false);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
                
                _logger.LogDebug(
                    "Message processed from {Queue}, DeliveryTag: {Tag}",
                    destination,
                    ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process message from {Queue}, DeliveryTag: {Tag}",
                    destination,
                    ea.DeliveryTag);
                
                // Requeue the message for retry
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        var consumerTag = channel.BasicConsume(
            queue: destination,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation(
            "RabbitMQ consumer started on queue: {Queue}, ConsumerTag: {Tag}",
            destination,
            consumerTag);

        // Handle cancellation
        if (ct.CanBeCanceled)
        {
            ct.Register(() =>
            {
                try
                {
                    _logger.LogInformation(
                        "Cancellation requested for queue: {Queue}, ConsumerTag: {Tag}",
                        destination,
                        consumerTag);
                    
                    channel.BasicCancel(consumerTag);
                    channel.Close();
                    channel.Dispose();
                    
                    _logger.LogInformation(
                        "Consumer gracefully stopped for queue: {Queue}",
                        destination);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Error while stopping consumer for queue: {Queue}",
                        destination);
                }
            });
        }

        return Task.CompletedTask;
    }
}

