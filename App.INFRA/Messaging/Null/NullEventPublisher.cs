using Microsoft.Extensions.Logging;

namespace App.INFRA.Messaging.Null;

/// <summary>
/// Null implementation of IEventPublisher that does nothing.
/// Used as a fallback when messaging is disabled.
/// </summary>
public sealed class NullEventPublisher : IEventPublisher
{
    private readonly ILogger<NullEventPublisher> _logger;

    public NullEventPublisher(ILogger<NullEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(
        string destination,
        string message,
        IDictionary<string, string>? headers = null,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "NullEventPublisher: Message to {Destination} was not sent (messaging disabled). Message length: {Length}",
            destination,
            message?.Length ?? 0);

        return Task.CompletedTask;
    }
}
