namespace App.INFRA.Messaging.Events;

/// <summary>
/// Event published when a new order is created
/// </summary>
public record OrderCreatedEvent
{
    public required Guid OrderId { get; init; }
    public required Guid UserId { get; init; }
    public required string OrderCode { get; init; }
    public required decimal TotalAmount { get; init; }
    public required string Currency { get; init; }
    public required string CustomerEmail { get; init; }
    public string? CustomerName { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string EventType => "order.created";
}

/// <summary>
/// Event published when an order status is updated
/// </summary>
public record OrderStatusChangedEvent
{
    public required Guid OrderId { get; init; }
    public required string OrderCode { get; init; }
    public required string OldStatus { get; init; }
    public required string NewStatus { get; init; }
    public required Guid ChangedByUserId { get; init; }
    public string? Reason { get; init; }
    public DateTime ChangedAt { get; init; } = DateTime.UtcNow;
    public string EventType => "order.status_changed";
}

/// <summary>
/// Event published when payment is completed
/// </summary>
public record PaymentCompletedEvent
{
    public required Guid OrderId { get; init; }
    public required Guid TransactionId { get; init; }
    public required string PaymentProvider { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public string? ExternalTransactionId { get; init; }
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
    public string EventType => "payment.completed";
}

/// <summary>
/// Event published to send email notification
/// </summary>
public record EmailNotificationEvent
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? TemplateName { get; init; }
    public Dictionary<string, string>? TemplateData { get; init; }
    public bool IsHtml { get; init; } = true;
    public DateTime QueuedAt { get; init; } = DateTime.UtcNow;
    public string EventType => "email.notification";
}

/// <summary>
/// Event published when inventory needs to be updated
/// </summary>
public record InventoryUpdateEvent
{
    public required Guid ProductId { get; init; }
    public required string ProductSku { get; init; }
    public required int QuantityChange { get; init; }
    public required string Reason { get; init; }
    public Guid? OrderId { get; init; }
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public string EventType => "inventory.update";
}
