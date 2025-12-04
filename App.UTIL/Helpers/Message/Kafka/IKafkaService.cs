namespace App.UTIL.Helpers.Message;

public interface IKafkaService
{
    string BootstrapServers { get; }
    string ClientId { get; }
    string TopicPrefix { get; }
    string BuildTopicName(string destination);
}

