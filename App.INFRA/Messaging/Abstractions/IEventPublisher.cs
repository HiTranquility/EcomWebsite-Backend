namespace App.INFRA.Messaging;

/// <summary>
/// Abstraction for event/message publishing to message brokers.
/// Supports RabbitMQ, Kafka, or any other message queue system.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a message to the specified destination.
    /// </summary>
    /// <param name="destination">Queue/Topic/Channel name</param>
    /// <param name="message">Message body (typically JSON)</param>
    /// <param name="headers">Optional message headers</param>
    /// <param name="ct">Cancellation token</param>
    Task PublishAsync(
        string destination,
        string message,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default);
}
