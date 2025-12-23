using RabbitMQ.Client;

namespace App.INFRA.Messaging;

/// <summary>
/// RabbitMQ service abstraction.
/// Provides connection management, configuration access, and queue naming utilities.
/// </summary>
public interface IRabbitMqService : IDisposable
{
    #region --- Configuration ---
    
    /// <summary>
    /// Whether RabbitMQ is enabled
    /// </summary>
    bool IsEnabled { get; }
    
    /// <summary>
    /// RabbitMQ server hostname
    /// </summary>
    string HostName { get; }
    
    /// <summary>
    /// RabbitMQ server port
    /// </summary>
    int Port { get; }
    
    /// <summary>
    /// Virtual host to connect to
    /// </summary>
    string VirtualHost { get; }
    
    /// <summary>
    /// Exchange name (if using exchanges)
    /// </summary>
    string? ExchangeName { get; }
    
    /// <summary>
    /// Exchange type (direct, fanout, topic, headers)
    /// </summary>
    string ExchangeType { get; }
    
    #endregion
    
    #region --- Connection ---
    
    /// <summary>
    /// Creates a new channel from the connection.
    /// Thread-safe and will auto-reconnect if connection is lost.
    /// </summary>
    IModel CreateChannel();
    
    #endregion
    
    #region --- Queue Naming ---
    
    /// <summary>
    /// Get queue name for a specific message type
    /// </summary>
    string GetQueueName(MessageQueueType queueType);
    
    #endregion
}

/// <summary>
/// Predefined message queue types for the application
/// </summary>
public enum MessageQueueType
{
    EmailNotification,
    OrderProcessing,
    PaymentCallback,
    Analytics,
    InventoryUpdate
}
