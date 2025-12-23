namespace App.INFRA.Messaging;

/// <summary>
/// Abstraction for event/message consumption from message brokers.
/// Supports RabbitMQ, Kafka, or any other message queue system.
/// </summary>
public interface IEventConsumer
{
    /// <summary>
    /// Subscribes to a queue/topic and processes messages with the provided handler.
    /// </summary>
    /// <param name="destination">Queue/Topic name to subscribe to</param>
    /// <param name="handler">Message handler function (message body, headers)</param>
    /// <param name="ct">Cancellation token for graceful shutdown</param>
    Task SubscribeAsync(
        string destination,
        Func<string, IDictionary<string, string>?, Task> handler,
        CancellationToken ct = default);
}
