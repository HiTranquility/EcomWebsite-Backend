namespace App.INFRA.Messaging;

/// <summary>
/// Abstraction for Kafka-specific service configuration.
/// Provides Kafka connection details and topic naming.
/// </summary>
public interface IKafkaService
{
    /// <summary>
    /// Kafka bootstrap servers (comma-separated list)
    /// </summary>
    string BootstrapServers { get; }
    
    /// <summary>
    /// Kafka client identifier
    /// </summary>
    string ClientId { get; }
    
    /// <summary>
    /// Prefix for all topic names
    /// </summary>
    string TopicPrefix { get; }
    
    /// <summary>
    /// Builds full topic name with prefix
    /// </summary>
    string BuildTopicName(string destination);
}
