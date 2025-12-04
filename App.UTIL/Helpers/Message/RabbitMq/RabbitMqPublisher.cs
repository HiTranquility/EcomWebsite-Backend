using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using App.UTIL.Helpers.Message.Schemas;

namespace App.UTIL.Helpers.Message;

public sealed class RabbitMqPublisher : IEventPublisher
{
    private readonly IEventBrokerConnection _connection;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IEventBrokerConnection connection, ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task PublishAsync(
        string destination,
        string message,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(message);

        ct.ThrowIfCancellationRequested();

        using IModel channel = _connection.CreateChannel();
        channel.QueueDeclare(
            queue: destination,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        IBasicProperties properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        if (headers is { Count: > 0 })
        {
            properties.Headers = headers.ToDictionary(static kvp => kvp.Key, static kvp => (object?)kvp.Value);
        }

        byte[] body = Encoding.UTF8.GetBytes(message);

        try
        {
            channel.BasicPublish(
                exchange: RabbitMqConfig.ExchangeName ?? string.Empty,
                routingKey: destination,
                basicProperties: properties,
                body: body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to {Destination}", destination);
            throw;
        }

        return Task.CompletedTask;
    }
}

