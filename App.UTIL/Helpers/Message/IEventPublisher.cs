namespace App.UTIL.Helpers.Message;

public interface IEventPublisher
{
    Task PublishAsync(
        string destination,              // topic / queue / channel name
        string message,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default);
}

