using Microsoft.Extensions.Logging;

namespace App.INFRA.Messaging.Kafka;

public sealed class KafkaConsumer : IEventConsumer
{
    private readonly IKafkaService _kafkaService;
    private readonly ILogger<KafkaConsumer> _logger;

    public KafkaConsumer(IKafkaService kafkaService, ILogger<KafkaConsumer> logger)
    {
        _kafkaService = kafkaService;
        _logger = logger;
    }

    public Task SubscribeAsync(
        string destination,
        Func<string, IDictionary<string, string>?, Task> handler,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(handler);

        string topic = _kafkaService.BuildTopicName(destination);
        _logger.LogWarning(
            "Kafka consumer stub: subscription requested for topic={Topic}. Implement actual consumer logic when Kafka client is available.",
            topic);

        return Task.CompletedTask;
    }
}

