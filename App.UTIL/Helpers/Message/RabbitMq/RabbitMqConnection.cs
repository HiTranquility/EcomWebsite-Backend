using App.UTIL.Helpers.Message.Schemas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace App.UTIL.Helpers.Message;

public sealed class RabbitMqConnection : IEventBrokerConnection
{
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnection(IConfiguration configuration, ILogger<RabbitMqConnection> logger)
    {

        _logger = logger;

        RabbitMqConfig.Init(configuration);
        if (!RabbitMqConfig.Enabled)
        {
            throw new InvalidOperationException("RabbitMQ is disabled via configuration.");
        }

        _factory = new ConnectionFactory
        {
            HostName = RabbitMqConfig.HostName,
            Port = RabbitMqConfig.Port,
            UserName = RabbitMqConfig.UserName,
            Password = RabbitMqConfig.Password,
            VirtualHost = RabbitMqConfig.VirtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };

        _connection = _factory.CreateConnection();
        using IModel channel = _connection.CreateModel();
        DeclareExchange(channel);
    }

    public IModel CreateChannel()
    {
        IConnection connection = EnsureConnection();
        return connection.CreateModel();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _connection?.Dispose();
        _connection = null;
    }

    private IConnection EnsureConnection()
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        _connection?.Dispose();
        _connection = _factory.CreateConnection();
        return _connection;
    }

    private void DeclareExchange(IModel channel)
    {
        if (string.IsNullOrWhiteSpace(RabbitMqConfig.ExchangeName))
        {
            return;
        }

        channel.ExchangeDeclare(
            exchange: RabbitMqConfig.ExchangeName,
            type: RabbitMqConfig.ExchangeType,
            durable: true,
            autoDelete: false,
            arguments: null);
    }
}

