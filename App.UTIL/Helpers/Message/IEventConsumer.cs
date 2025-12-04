namespace App.UTIL.Helpers.Message;

public interface IEventConsumer
{
    Task SubscribeAsync(
        string destination,
        Func<string, IDictionary<string, string>?, Task> handler,
        CancellationToken ct = default);
}

