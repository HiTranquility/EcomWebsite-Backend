using System.Text.Json;
using App.INFRA.Messaging.Events;
using App.UTIL.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace App.INFRA.Messaging.RabbitMq.Workers;

/// <summary>
/// Background worker that consumes email notification events from RabbitMQ
/// and processes them (e.g., sends emails via SMTP).
/// </summary>
public sealed class EmailNotificationWorker : BackgroundService
{
    private readonly IEventConsumer _consumer;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<EmailNotificationWorker> _logger;
    // private readonly IEmailService _emailService; // Inject when ready

    public EmailNotificationWorker(
        IEventConsumer consumer,
        IOptions<RabbitMqSettings> options,
        ILogger<EmailNotificationWorker> logger)
        // IEmailService emailService)
    {
        _consumer = consumer;
        _settings = options.Value;
        _logger = logger;
        // _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("EmailNotificationWorker: RabbitMQ is disabled, worker will not start");
            return;
        }

        _logger.LogInformation(
            "EmailNotificationWorker starting, listening on queue: {Queue}",
            _settings.Queues.EmailNotification);

        try
        {
            await _consumer.SubscribeAsync(
                destination: _settings.Queues.EmailNotification,
                handler: HandleEmailNotificationAsync,
                ct: stoppingToken);

            // Keep the worker alive
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("EmailNotificationWorker: Graceful shutdown requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EmailNotificationWorker: Unhandled exception");
            throw;
        }
    }

    private async Task HandleEmailNotificationAsync(
        string message,
        IDictionary<string, string>? headers)
    {
        try
        {
            var emailEvent = JsonSerializer.Deserialize<EmailNotificationEvent>(message);
            if (emailEvent == null)
            {
                _logger.LogWarning("EmailNotificationWorker: Failed to deserialize message");
                return;
            }

            _logger.LogInformation(
                "EmailNotificationWorker: Processing email to {To}, Subject: {Subject}",
                emailEvent.To,
                emailEvent.Subject);

            // TODO: Actually send email via _emailService
            // await _emailService.SendEmailAsync(
            //     emailEvent.To,
            //     emailEvent.Subject,
            //     emailEvent.Body,
            //     emailEvent.IsHtml);

            // Simulate email sending delay
            await Task.Delay(100);

            _logger.LogInformation(
                "EmailNotificationWorker: Email sent successfully to {To}",
                emailEvent.To);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "EmailNotificationWorker: JSON deserialization failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EmailNotificationWorker: Failed to process email notification");
            throw; // Rethrow to trigger NACK and requeue
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailNotificationWorker: Stopping...");
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("EmailNotificationWorker: Stopped");
    }
}
