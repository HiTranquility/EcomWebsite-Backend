using Microsoft.Extensions.Configuration;

namespace App.UTIL.Helpers.Message.Schemas;

public static class RabbitMqConfig
{
    private static bool _initialized;

    public static bool Enabled { get; private set; } = true;
    public static string HostName { get; private set; } = "localhost";
    public static int Port { get; private set; } = 5672;
    public static string UserName { get; private set; } = "guest";
    public static string Password { get; private set; } = "guest";
    public static string VirtualHost { get; private set; } = "/";
    public static string? ExchangeName { get; private set; }
    public static string ExchangeType { get; private set; } = global::RabbitMQ.Client.ExchangeType.Direct;

    public static void Init(IConfiguration configuration)
    {
        if (_initialized)
        {
            return;
        }

        IConfigurationSection section = configuration.GetSection("RabbitMqSettings");
        if (section == null || !section.Exists())
        {
            throw new InvalidOperationException("Missing RabbitMqSettings section in configuration.");
        }

        Enabled = section.GetValue("Enabled", true);
        HostName = section["HostName"] ?? HostName;
        UserName = section["UserName"] ?? UserName;
        Password = section["Password"] ?? Password;
        VirtualHost = section["VirtualHost"] ?? VirtualHost;
        Port = section.GetValue("Port", Port);
        ExchangeName = section["ExchangeName"] ?? ExchangeName;
        ExchangeType = section["ExchangeType"] ?? ExchangeType;

        _initialized = true;
    }
}

