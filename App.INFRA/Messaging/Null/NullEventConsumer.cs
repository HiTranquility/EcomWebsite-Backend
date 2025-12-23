using Microsoft.Extensions.Logging;

namespace App.INFRA.Messaging.Null;

/// <summary>
/// Null implementation of IEventConsumer that does nothing.
/// Used as a fallback when messaging is disabled.
/// </summary>
public sealed class NullEventConsumer : IEventConsumer
{
    private readonly ILogger<NullEventConsumer> _logger;

    public NullEventConsumer(ILogger<NullEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task SubscribeAsync(
        string destination,
        Func<string, IDictionary<string, string>?, Task> handler,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "NullEventConsumer: Subscription to {Destination} was not created (messaging disabled)",
            destination);

        return Task.CompletedTask;
    }
}
