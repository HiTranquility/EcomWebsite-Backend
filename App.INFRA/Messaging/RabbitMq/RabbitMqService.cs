using App.UTIL.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace App.INFRA.Messaging.RabbitMq;

/// <summary>
/// Singleton RabbitMQ service.
/// Provides configuration, connection management, and queue naming utilities.
/// Thread-safe with automatic connection recovery.
/// </summary>
public sealed class RabbitMqService : IRabbitMqService
{
    private readonly ILogger<RabbitMqService> _logger;
    private readonly RabbitMqSettings _settings;
    private readonly ConnectionFactory _factory;
    private readonly object _connectionLock = new();
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqService(
        IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqService> logger)
    {
        _logger = logger;
        _settings = options.Value;
        
        _factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(_settings.ConnectionTimeoutSeconds)
        };

        _logger.LogInformation(
            "RabbitMQ service configured for {Host}:{Port}, VirtualHost: {VHost}",
            _settings.HostName,
            _settings.Port,
            _settings.VirtualHost);
    }

    #region --- IRabbitMqService Configuration Properties ---
    
    public bool IsEnabled => _settings.Enabled;
    public string HostName => _settings.HostName;
    public int Port => _settings.Port;
    public string VirtualHost => _settings.VirtualHost;
    public string? ExchangeName => _settings.ExchangeName;
    public string ExchangeType => _settings.ExchangeType;
    
    #endregion

    #region --- Connection Management ---

    /// <summary>
    /// Creates a new channel from the connection.
    /// Thread-safe and will auto-reconnect if connection is lost.
    /// </summary>
    public IModel CreateChannel()
    {
        var connection = EnsureConnection();
        var channel = connection.CreateModel();
        
        // Apply QoS settings
        channel.BasicQos(prefetchSize: 0, prefetchCount: _settings.PrefetchCount, global: false);
        
        // Declare exchange if configured
        DeclareExchangeIfNeeded(channel);
        
        return channel;
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_connectionLock)
        {
            if (_disposed) return;
            _disposed = true;
            
            if (_connection != null)
            {
                try
                {
                    _connection.Close(TimeSpan.FromSeconds(5));
                    _connection.Dispose();
                    _logger.LogInformation("RabbitMQ connection closed gracefully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while closing RabbitMQ connection");
                }
                finally
                {
                    _connection = null;
                }
            }
        }
    }

    private IConnection EnsureConnection()
    {
        // Fast path: connection is open
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        // Slow path: need to create/recreate connection
        lock (_connectionLock)
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            // Dispose old connection if exists
            if (_connection != null)
            {
                try { _connection.Dispose(); }
                catch (Exception ex) { _logger.LogWarning(ex, "Error disposing old RabbitMQ connection"); }
            }

            // Create new connection
            _connection = _factory.CreateConnection();
            
            _logger.LogInformation(
                "RabbitMQ connected to {Host}:{Port}",
                _settings.HostName,
                _settings.Port);

            return _connection;
        }
    }

    private void DeclareExchangeIfNeeded(IModel channel)
    {
        if (string.IsNullOrWhiteSpace(_settings.ExchangeName)) return;

        var exchangeType = _settings.ExchangeType switch
        {
            "fanout" => RabbitMQ.Client.ExchangeType.Fanout,
            "topic" => RabbitMQ.Client.ExchangeType.Topic,
            "headers" => RabbitMQ.Client.ExchangeType.Headers,
            _ => RabbitMQ.Client.ExchangeType.Direct
        };

        channel.ExchangeDeclare(
            exchange: _settings.ExchangeName,
            type: exchangeType,
            durable: true,
            autoDelete: false,
            arguments: null);

        _logger.LogDebug(
            "RabbitMQ exchange declared: {Exchange} (type: {Type})",
            _settings.ExchangeName,
            exchangeType);
    }
    
    #endregion

    #region --- Queue Naming ---

    public string GetQueueName(MessageQueueType queueType)
    {
        return queueType switch
        {
            MessageQueueType.EmailNotification => _settings.Queues.EmailNotification,
            MessageQueueType.OrderProcessing => _settings.Queues.OrderProcessing,
            MessageQueueType.PaymentCallback => _settings.Queues.PaymentCallback,
            MessageQueueType.Analytics => _settings.Queues.Analytics,
            MessageQueueType.InventoryUpdate => _settings.Queues.InventoryUpdate,
            _ => throw new ArgumentOutOfRangeException(nameof(queueType), queueType, "Unknown queue type")
        };
    }
    
    #endregion
}
