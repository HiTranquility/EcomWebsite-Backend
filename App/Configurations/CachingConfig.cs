using Microsoft.Extensions.Caching.Hybrid;
namespace App.Configurations;

public static class CachingConfig
{
    public static IServiceCollection ConfigureHybridCache(this IServiceCollection services, IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool>("Enabled");
        if (!enabled)
        {
            services.AddHybridCache();
            return services;
        }
        else
        {
            var expirationSeconds = configuration.GetValue<int>("ExpirationSeconds");
            var localExpirationSeconds = configuration.GetValue<int>("LocalExpirationSeconds");
            var flags = configuration.GetValue<HybridCacheEntryFlags>("Flags");
            services.AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(expirationSeconds),
                    LocalCacheExpiration = TimeSpan.FromSeconds(localExpirationSeconds),
                    Flags = flags
                };
            });

            return services;
        }
        
    }

    public static IServiceCollection ConfigureDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetValue<string>("Redis:ConnectionString");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
        });
        return services;
    }
    public static IServiceCollection ConfigureMemoryCache(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}