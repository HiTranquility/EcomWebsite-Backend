namespace App.UTIL.Settings;

/// <summary>
/// Configuration settings for RabbitMQ message broker.
/// Maps to "RabbitMqSettings" section in appsettings.json
/// </summary>
public class RabbitMqSettings
{
    public const string SectionName = "RabbitMqSettings";

    /// <summary>
    /// Enable or disable RabbitMQ messaging
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Broker type identifier (e.g., "RabbitMQ", "Kafka")
    /// </summary>
    public string BrokerType { get; set; } = "RabbitMQ";

    /// <summary>
    /// RabbitMQ server hostname
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ server port (default: 5672)
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Username for authentication
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Password for authentication
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Virtual host (default: "/")
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Exchange type: direct, fanout, topic, headers
    /// </summary>
    public string ExchangeType { get; set; } = "direct";

    /// <summary>
    /// Default exchange name
    /// </summary>
    public string? ExchangeName { get; set; }

    /// <summary>
    /// Number of retry attempts for failed messages
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Delay between retries in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Number of messages to prefetch (QoS)
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Queue configurations
    /// </summary>
    public RabbitMqQueueSettings Queues { get; set; } = new();
}

/// <summary>
/// Queue name configurations
/// </summary>
public class RabbitMqQueueSettings
{
    /// <summary>
    /// Queue for email notifications
    /// </summary>
    public string EmailNotification { get; set; } = "email.notifications";

    /// <summary>
    /// Queue for order processing events
    /// </summary>
    public string OrderProcessing { get; set; } = "order.processing";

    /// <summary>
    /// Queue for payment callback events
    /// </summary>
    public string PaymentCallback { get; set; } = "payment.callbacks";

    /// <summary>
    /// Queue for analytics events
    /// </summary>
    public string Analytics { get; set; } = "analytics.events";

    /// <summary>
    /// Queue for inventory updates
    /// </summary>
    public string InventoryUpdate { get; set; } = "inventory.updates";
}
