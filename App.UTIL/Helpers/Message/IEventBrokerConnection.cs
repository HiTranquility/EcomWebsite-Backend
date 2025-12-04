using RabbitMQ.Client;

namespace App.UTIL.Helpers.Message;

public interface IEventBrokerConnection : IDisposable
{
    IModel CreateChannel();
}

