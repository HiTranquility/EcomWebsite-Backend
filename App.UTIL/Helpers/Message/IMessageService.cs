namespace App.UTIL.Helpers.Message;

public interface IMessageService
{
    Task PublishAsync(string topic, string message);
    Task SubscribeAsync(string topic, Func<string, Task> handler);
}