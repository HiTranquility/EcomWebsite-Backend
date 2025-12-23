using App.INFRA.BackgroundJobs;
using App.INFRA.Messaging;
using App.INFRA.Messaging.Kafka;
using App.INFRA.Messaging.Null;
using App.INFRA.Messaging.RabbitMq;
using App.INFRA.Messaging.RabbitMq.Workers;
using App.UTIL.Settings;
using Hangfire;
using Hangfire.MySql;
using Microsoft.Extensions.Caching.Hybrid;


namespace App.Configurations;

/// <summary>
/// Configurations for Infrastructure services (Cache, Queue, Payment, Background Jobs).
/// Consolidated from previous separate config files.
/// </summary>
public static class InfrastructureConfig
{
    #region --- Caching Configuration ---

    public static IServiceCollection ConfigureHybridCache(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheSection = configuration.GetSection("CacheSettings");
        var redisSection = configuration.GetSection("Redis");

        var enabled = cacheSection.GetValue<bool?>("Enabled") ?? false;
        var redisConnection = redisSection.GetValue<string>("ConnectionString");
        var redisInstanceName = redisSection.GetValue<string>("InstanceName");

        // Configure Redis distributed cache if connection string provided
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection));
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                if (!string.IsNullOrWhiteSpace(redisInstanceName)) options.InstanceName = redisInstanceName;
            });
        }

        // HybridCache configuration
        var expirationSeconds = cacheSection.GetValue<int?>("ExpirationSeconds") ?? 60;
        var localExpSeconds = cacheSection.GetValue<int?>("LocalCacheExpirationSeconds") ?? 30;
        var maxEntriesInMemory = cacheSection.GetValue<int?>("MaxEntriesInMemory") ?? 1000;
        var flagsStr = cacheSection.GetValue<string>("Flags") ?? "None";
        var flags = Enum.TryParse<HybridCacheEntryFlags>(flagsStr, true, out var f) ? f : HybridCacheEntryFlags.None;

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(expirationSeconds),
                LocalCacheExpiration = TimeSpan.FromSeconds(localExpSeconds),
                Flags = flags
            };
            
            // Memory size limits to prevent unbounded growth
            options.MaximumPayloadBytes = 1024 * 1024; // 1MB max per entry
            options.MaximumKeyLength = 512;
        });

        return services;
    }

    public static IServiceCollection ConfigureDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetValue<string>("Redis:ConnectionString");
        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
        return services;
    }

    #endregion

    #region --- Messaging (RabbitMQ/Kafka) ---

    /// <summary>
    /// Configure messaging services with conditional registration.
    /// Registers RabbitMQ/Kafka services when enabled, or null implementations as fallback.
    /// </summary>
    public static IServiceCollection ConfigureMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure RabbitMQ settings (always needed for IOptions)
        var rabbitSection = configuration.GetSection("RabbitMqSettings");
        services.Configure<RabbitMqSettings>(options =>
        {
            options.Enabled = rabbitSection.GetValue<bool>("Enabled");
            options.BrokerType = rabbitSection.GetValue<string>("BrokerType") ?? "RabbitMQ";
            options.HostName = rabbitSection.GetValue<string>("HostName") ?? "localhost";
            options.Port = rabbitSection.GetValue<int?>("Port") ?? 5672;
            options.UserName = rabbitSection.GetValue<string>("UserName") ?? "guest";
            options.Password = rabbitSection.GetValue<string>("Password") ?? "guest";
            options.VirtualHost = rabbitSection.GetValue<string>("VirtualHost") ?? "/";
            options.ExchangeType = rabbitSection.GetValue<string>("ExchangeType") ?? "direct";
            options.ExchangeName = rabbitSection.GetValue<string>("ExchangeName");
            options.RetryCount = rabbitSection.GetValue<int?>("RetryCount") ?? 3;
            options.RetryDelayMs = rabbitSection.GetValue<int?>("RetryDelayMs") ?? 1000;
            options.PrefetchCount = (ushort)(rabbitSection.GetValue<int?>("PrefetchCount") ?? 10);
            options.ConnectionTimeoutSeconds = rabbitSection.GetValue<int?>("ConnectionTimeoutSeconds") ?? 30;

            var qSection = rabbitSection.GetSection("Queues");
            options.Queues = new RabbitMqQueueSettings
            {
                EmailNotification = qSection.GetValue<string>("EmailNotification") ?? "email.notifications",
                OrderProcessing = qSection.GetValue<string>("OrderProcessing") ?? "order.processing",
                PaymentCallback = qSection.GetValue<string>("PaymentCallback") ?? "payment.callbacks",
                Analytics = qSection.GetValue<string>("Analytics") ?? "analytics.events",
                InventoryUpdate = qSection.GetValue<string>("InventoryUpdate") ?? "inventory.updates"
            };
        });
        
        // Conditional service registration based on enabled broker
        var rabbitEnabled = configuration.GetValue<bool>("RabbitMqSettings:Enabled");
        var kafkaEnabled = configuration.GetValue<bool>("KafkaSettings:Enabled");
        
        if (rabbitEnabled)
        {
            // RabbitMQ enabled
            services.AddSingleton<IRabbitMqService,  RabbitMqService>();
            services.AddSingleton<IEventPublisher,  RabbitMqPublisher>();
            services.AddSingleton<IEventConsumer,  RabbitMqConsumer>();
        }
        else if (kafkaEnabled)
        {
            // Kafka enabled
            services.AddSingleton<IKafkaService, KafkaService>();
            services.AddSingleton<IEventPublisher, KafkaPublisher>();
            services.AddSingleton<IEventConsumer, KafkaConsumer>();
        }
        else
        {
            // No messaging - use null implementations (no-op)
            services.AddSingleton<IEventPublisher, NullEventPublisher>();
            services.AddSingleton<IEventConsumer, NullEventConsumer>();
        }
        
        return services;
    }

    /// <summary>
    /// Add RabbitMQ background workers (only when enabled).
    /// </summary>
    public static IServiceCollection AddRabbitMqWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool>("RabbitMqSettings:Enabled");
        if (enabled)
        {
            services.AddHostedService<EmailNotificationWorker>();
            services.AddHostedService<OrderProcessingWorker>();
        }
        return services;
    }

    #endregion

    #region --- Payment ---

    public static IServiceCollection ConfigurePayment(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StripeSettings>(o =>
        {
            var s = configuration.GetSection("StripeSettings");
            o.SecretKey = s.GetValue<string>("SecretKey") ?? "";
            o.PublishableKey = s.GetValue<string>("PublishableKey") ?? "";
            o.WebhookSecret = s.GetValue<string>("WebhookSecret") ?? "";
            o.Currency = s.GetValue<string>("Currency") ?? "usd";
        });

        services.Configure<MoMoSettings>(o =>
        {
            var s = configuration.GetSection("MoMoSettings");
            o.PartnerCode = s.GetValue<string>("PartnerCode") ?? "";
            o.AccessKey = s.GetValue<string>("AccessKey") ?? "";
            o.SecretKey = s.GetValue<string>("SecretKey") ?? "";
            o.Endpoint = s.GetValue<string>("Endpoint") ?? "https://test-payment.momo.vn/v2/gateway/api";
            o.IpnUrl = s.GetValue<string>("IpnUrl") ?? "";
        });

        services.Configure<VNPaySettings>(o =>
        {
            var s = configuration.GetSection("VNPaySettings");
            o.TmnCode = s.GetValue<string>("TmnCode") ?? "";
            o.HashSecret = s.GetValue<string>("HashSecret") ?? "";
            o.PaymentUrl = s.GetValue<string>("PaymentUrl") ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            o.ReturnUrl = s.GetValue<string>("ReturnUrl") ?? "";
            o.Version = s.GetValue<string>("Version") ?? "2.1.0";
            o.Command = s.GetValue<string>("Command") ?? "pay";
            o.CurrCode = s.GetValue<string>("CurrCode") ?? "VND";
            o.Locale = s.GetValue<string>("Locale") ?? "vn";
        });

        services.AddHttpClient();
        return services;
    }

    #endregion

    #region --- Background Jobs (Hangfire) ---

    public static IServiceCollection ConfigureHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("Hangfire");
        var useInMemory = string.IsNullOrWhiteSpace(conn) || conn.Contains("${"); // Placeholder check remains for connection string

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180).UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings();
            
            if (useInMemory) config.UseInMemoryStorage();
            else config.UseStorage(new MySqlStorage(conn, new MySqlStorageOptions
            {
                TablesPrefix = "hangfire_",
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 50000,
                TransactionTimeout = TimeSpan.FromMinutes(1)
            }));
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = ["default", "critical", "emails", "reports"];
        });

        return services;
    }

    public static WebApplication UseHangfireDashboard(this WebApplication app, IConfiguration configuration)
    {
        var dashboardOptions = new DashboardOptions
        {
            DashboardTitle = "EcomWebsite - Background Jobs",
            DisplayStorageConnectionString = false
        };

        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.MapHangfireDashboard("/hangfire", dashboardOptions);
        }

        ConfigureRecurringJobs();
        return app;
    }

    private static void ConfigureRecurringJobs()
    {
        RecurringJob.AddOrUpdate<IBackgroundJobService>("cache-cleanup", s => s.CleanupExpiredCacheAsync(), Cron.Daily(3, 0));
        RecurringJob.AddOrUpdate<IBackgroundJobService>("order-status-check", s => s.CheckPendingOrdersAsync(), Cron.Hourly);
        RecurringJob.AddOrUpdate<IBackgroundJobService>("weekly-report", s => s.GenerateWeeklyReportAsync(), Cron.Weekly(DayOfWeek.Monday, 6, 0));
        RecurringJob.AddOrUpdate<IBackgroundJobService>("monthly-cleanup", s => s.MonthlyDataCleanupAsync(), Cron.Monthly(1, 4, 0));
    }

    #endregion
}
