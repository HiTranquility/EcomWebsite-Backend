using Microsoft.Extensions.Configuration;

namespace App.INFRA.Messaging.Kafka;

public sealed class KafkaService : IKafkaService
{
    private readonly string _bootstrapServers;
    private readonly string _clientId;
    private readonly string _topicPrefix;

    public KafkaService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        IConfigurationSection section = configuration.GetSection("KafkaSettings");
        if (section == null || !section.Exists())
        {
            throw new InvalidOperationException("Missing KafkaSettings section in configuration.");
        }

        bool enabled = section.GetValue("Enabled", false);
        if (!enabled)
        {
            throw new InvalidOperationException("Kafka is disabled via configuration.");
        }

        _bootstrapServers = section["BootstrapServers"] ?? throw new InvalidOperationException("Missing KafkaSettings:BootstrapServers");
        _clientId = section["ClientId"] ?? throw new InvalidOperationException("Missing KafkaSettings:ClientId");
        _topicPrefix = section["TopicPrefix"] ?? "app";
    }

    public string BootstrapServers => _bootstrapServers;
    public string ClientId => _clientId;
    public string TopicPrefix => _topicPrefix;

    public string BuildTopicName(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException("Destination topic is required.", nameof(destination));
        }

        return $"{_topicPrefix}.{destination}".ToLowerInvariant();
    }
}

