namespace App.UTIL.Helpers.Message;

public class KafkaAdapter : IMessageService
{
    public Task PublishAsync(string topic, string message)
    {
        // TODO: implement Kafka publish logic
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(string topic, Func<string, Task> handler)
    {
        // TODO: implement Kafka subscribe logic
        return Task.CompletedTask;
    }
}