namespace App.UTIL.Helpers.Message;

public class RabbitMqAdapter : IMessageService
{
    public Task PublishAsync(string topic, string message)
    {
        // TODO: implement RabbitMQ publish logic
        return Task.CompletedTask;
    }

    public Task SubscribeAsync(string topic, Func<string, Task> handler)
    {
        // TODO: implement RabbitMQ subscribe logic
        return Task.CompletedTask;
    }
}