using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace App.Configurations;

/// <summary>
/// Health Checks configuration for monitoring application dependencies.
/// Provides endpoints for liveness, readiness, and detailed health status.
/// </summary>
public static class HealthCheckConfig
{
    /// <summary>
    /// Configure health checks for all application dependencies.
    /// </summary>
    public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks()
            // Self check - always healthy if app is running
            .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"), tags: ["live"]);

        // MySQL Health Checks - Check all database contexts
        ConfigureMySqlHealthChecks(healthChecksBuilder, configuration);
        
        // Redis Health Check
        ConfigureRedisHealthCheck(healthChecksBuilder, configuration);
        
        // RabbitMQ Health Check
        ConfigureRabbitMqHealthCheck(healthChecksBuilder, configuration);
        
        // Payment Services Health Checks
        ConfigurePaymentHealthChecks(healthChecksBuilder, configuration);

        return services;
    }
    
    private static void ConfigureMySqlHealthChecks(IHealthChecksBuilder builder, IConfiguration configuration)
    {
        // Sử dụng GetConnectionString() - đơn giản, clean
        AddDbCheck(builder, configuration, "MyUserSqlConn", "mysql-users");
        AddDbCheck(builder, configuration, "MyProductSqlConn", "mysql-products");
        AddDbCheck(builder, configuration, "MyOrderSqlConn", "mysql-orders");
        AddDbCheck(builder, configuration, "MyBlogSqlConn", "mysql-blogs");
    }

    private static void AddDbCheck(IHealthChecksBuilder builder, IConfiguration configuration, string connName, string healthName)
    {
        var connection = configuration.GetConnectionString(connName);
        if (!string.IsNullOrWhiteSpace(connection))
        {
            builder.AddMySql(
                connectionString: connection,
                name: healthName,
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "mysql"]);
        }
    }
    
    private static void ConfigureRedisHealthCheck(IHealthChecksBuilder builder, IConfiguration configuration)
    {
        // Đơn giản hóa việc đọc chuỗi kết nối Redis
        var redisConnection = configuration.GetValue<string>("Redis:ConnectionString");
        
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            builder.AddRedis(
                redisConnectionString: redisConnection,
                name: "redis",
                failureStatus: HealthStatus.Degraded, // Cache chết thì app vẫn chạy được
                tags: ["ready", "cache", "redis"]);
        }
    }
    
    private static void ConfigureRabbitMqHealthCheck(IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var rabbitEnabled = configuration.GetValue<bool>("RabbitMqSettings:Enabled");
        if (!rabbitEnabled) return;
        
        // Build connection URI from settings
        var host = configuration.GetValue<string>("RabbitMqSettings:HostName") ?? "localhost";
        var port = configuration.GetValue<int?>("RabbitMqSettings:Port") ?? 5672;
        var user = configuration.GetValue<string>("RabbitMqSettings:UserName") ?? "guest";
        var pass = configuration.GetValue<string>("RabbitMqSettings:Password") ?? "guest";
        var vhost = configuration.GetValue<string>("RabbitMqSettings:VirtualHost") ?? "/";
        
        var connectionString = $"amqp://{user}:{pass}@{host}:{port}{vhost}";
        
        builder.AddRabbitMQ(
            _ => new RabbitMQ.Client.ConnectionFactory { Uri = new Uri(connectionString) }.CreateConnectionAsync(),
            name: "rabbitmq", 
            failureStatus: HealthStatus.Degraded,
            tags: ["ready", "messaging", "rabbitmq"]);
    }
    
    private static void ConfigurePaymentHealthChecks(IHealthChecksBuilder builder, IConfiguration configuration)
    {
        // Stripe API Health Check
        var stripeKey = configuration.GetValue<string>("StripeSettings:SecretKey") 
                     ?? configuration.GetValue<string>("Stripe:SecretKey");
            
        if (!string.IsNullOrWhiteSpace(stripeKey))
        {
            builder.AddUrlGroup(
                new Uri("https://api.stripe.com/v1/"), 
                name: "stripe-api",
                failureStatus: HealthStatus.Degraded,
                tags: ["ready", "payment", "external"]);
        }
    }

    
    /// <summary>
    /// Map health check endpoints in the application pipeline.
    /// </summary>
    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // Liveness probe - is the app running?
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        // Readiness probe - is the app ready to serve requests?
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        // Detailed health - all checks
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        // Database specific health
        app.MapHealthChecks("/health/db", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        return app;
    }
}
