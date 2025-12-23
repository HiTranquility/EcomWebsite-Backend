using System.Text.Json;
using App.INFRA.Messaging.Events;
using App.UTIL.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace App.INFRA.Messaging.RabbitMq.Workers;

/// <summary>
/// Background worker that consumes order processing events from RabbitMQ.
/// Handles order confirmation emails, inventory updates, etc.
/// </summary>
public sealed class OrderProcessingWorker : BackgroundService
{
    private readonly IEventConsumer _consumer;
    private readonly IEventPublisher _publisher;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<OrderProcessingWorker> _logger;

    public OrderProcessingWorker(
        IEventConsumer consumer,
        IEventPublisher publisher,
        IOptions<RabbitMqSettings> options,
        ILogger<OrderProcessingWorker> logger)
    {
        _consumer = consumer;
        _publisher = publisher;
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("OrderProcessingWorker: RabbitMQ is disabled, worker will not start");
            return;
        }

        _logger.LogInformation(
            "OrderProcessingWorker starting, listening on queue: {Queue}",
            _settings.Queues.OrderProcessing);

        try
        {
            await _consumer.SubscribeAsync(
                destination: _settings.Queues.OrderProcessing,
                handler: HandleOrderEventAsync,
                ct: stoppingToken);

            // Keep the worker alive
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("OrderProcessingWorker: Graceful shutdown requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OrderProcessingWorker: Unhandled exception");
            throw;
        }
    }

    private async Task HandleOrderEventAsync(
        string message,
        IDictionary<string, string>? headers)
    {
        string eventType = "unknown";
        if (headers != null && headers.TryGetValue("eventType", out var headerValue))
        {
            eventType = headerValue;
        }
        
        _logger.LogInformation(
            "OrderProcessingWorker: Received event type: {EventType}",
            eventType);

        try
        {
            switch (eventType)
            {
                case "order.created":
                    await HandleOrderCreatedAsync(message);
                    break;
                    
                case "order.status_changed":
                    await HandleOrderStatusChangedAsync(message);
                    break;
                    
                default:
                    _logger.LogWarning(
                        "OrderProcessingWorker: Unknown event type: {EventType}",
                        eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OrderProcessingWorker: Failed to process event {EventType}", eventType);
            throw; // Rethrow to trigger NACK and requeue
        }
    }

    private async Task HandleOrderCreatedAsync(string message)
    {
        var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
        if (orderEvent == null)
        {
            _logger.LogWarning("OrderProcessingWorker: Failed to deserialize OrderCreatedEvent");
            return;
        }

        _logger.LogInformation(
            "OrderProcessingWorker: Processing new order {OrderCode} for customer {Email}",
            orderEvent.OrderCode,
            orderEvent.CustomerEmail);

        // 1. Send order confirmation email
        var emailEvent = new EmailNotificationEvent
        {
            To = orderEvent.CustomerEmail,
            Subject = $"Order Confirmation - {orderEvent.OrderCode}",
            Body = $"""
                <h1>Thank you for your order!</h1>
                <p>Your order <strong>{orderEvent.OrderCode}</strong> has been received.</p>
                <p>Total Amount: {orderEvent.TotalAmount:N0} {orderEvent.Currency}</p>
                <p>We will notify you when your order ships.</p>
                """,
            IsHtml = true,
            TemplateName = "order_confirmation",
            TemplateData = new Dictionary<string, string>
            {
                ["orderCode"] = orderEvent.OrderCode,
                ["totalAmount"] = orderEvent.TotalAmount.ToString("N0"),
                ["currency"] = orderEvent.Currency,
                ["customerName"] = orderEvent.CustomerName ?? "Customer"
            }
        };

        await _publisher.PublishAsync(
            destination: _settings.Queues.EmailNotification,
            message: JsonSerializer.Serialize(emailEvent),
            headers: new Dictionary<string, string>
            {
                ["eventType"] = emailEvent.EventType,
                ["correlationId"] = orderEvent.OrderId.ToString()
            });

        _logger.LogInformation(
            "OrderProcessingWorker: Order confirmation email queued for {Email}",
            orderEvent.CustomerEmail);
    }

    private async Task HandleOrderStatusChangedAsync(string message)
    {
        var statusEvent = JsonSerializer.Deserialize<OrderStatusChangedEvent>(message);
        if (statusEvent == null)
        {
            _logger.LogWarning("OrderProcessingWorker: Failed to deserialize OrderStatusChangedEvent");
            return;
        }

        _logger.LogInformation(
            "OrderProcessingWorker: Order {OrderCode} status changed from {OldStatus} to {NewStatus}",
            statusEvent.OrderCode,
            statusEvent.OldStatus,
            statusEvent.NewStatus);

        // TODO: Handle status change (e.g., send notification, update inventory)
        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OrderProcessingWorker: Stopping...");
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("OrderProcessingWorker: Stopped");
    }
}
