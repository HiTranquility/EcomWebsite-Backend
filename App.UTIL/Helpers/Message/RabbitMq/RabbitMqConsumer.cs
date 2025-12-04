using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace App.UTIL.Helpers.Message;

public sealed class RabbitMqConsumer : IEventConsumer
{
    private readonly IEventBrokerConnection _connection;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(IEventBrokerConnection connection, ILogger<RabbitMqConsumer> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task SubscribeAsync(
        string destination,
        Func<string, IDictionary<string, string>?, Task> handler,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(handler);
        ct.ThrowIfCancellationRequested();

        IModel channel = _connection.CreateChannel();
        channel.QueueDeclare(
            queue: destination,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.Received += async (_, ea) =>
        {
            string payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            IDictionary<string, string>? headerValues = null;

            if (ea.BasicProperties?.Headers is { Count: > 0 } rawHeaders)
            {
                headerValues = new Dictionary<string, string>(rawHeaders.Count, StringComparer.OrdinalIgnoreCase);
                foreach ((string key, object? value) in rawHeaders)
                {
                    headerValues[key] = value switch
                    {
                        byte[] bytes => Encoding.UTF8.GetString(bytes),
                        string str => str,
                        null => string.Empty,
                        _ => value?.ToString() ?? string.Empty
                    };
                }
            }

            try
            {
                await handler(payload, headerValues).ConfigureAwait(false);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ consumer failed for queue {Queue}", destination);
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        string consumerTag = channel.BasicConsume(
            queue: destination,
            autoAck: false,
            consumer: consumer);

        if (ct.CanBeCanceled)
        {
            ct.Register(() =>
            {
                try
                {
                    channel.BasicCancel(consumerTag);
                    channel.Close();
                    channel.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cancel RabbitMQ consumer for queue {Queue}", destination);
                }
            });
        }

        return Task.CompletedTask;
    }
}

