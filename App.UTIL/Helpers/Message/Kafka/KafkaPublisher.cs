using Microsoft.Extensions.Logging;

namespace App.UTIL.Helpers.Message;

public sealed class KafkaPublisher : IEventPublisher
{
    private readonly ILogger<KafkaPublisher> _logger;

    public KafkaPublisher(ILogger<KafkaPublisher> logger)
    {
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

        _logger.LogWarning("Kafka publisher not configured; skipping publish to {Destination}", destination);
        return Task.CompletedTask;
    }
}
