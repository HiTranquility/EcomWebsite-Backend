using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace App.Configurations;

public static class CachingConfig
{
    public static IServiceCollection ConfigureHybridCache(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheSection = configuration.GetSection("CacheSettings");
        var enabled = cacheSection.GetValue<bool>("Enabled");
        
        // ✅ Thêm Redis backend cho HybridCache (nếu có config)
        var redisSection = configuration.GetSection("Redis");
        var redisConnection = redisSection.GetValue<string>("ConnectionString");
        var redisInstanceName = redisSection.GetValue<string>("InstanceName");
        
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            // ✅ Lưu ConnectionMultiplexer vào DI để dùng cho instrumentation
            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
            {
                return StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection);
            });
            
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                if (!string.IsNullOrWhiteSpace(redisInstanceName))
                {
                    options.InstanceName = redisInstanceName;
                }
            });
        }
        
        if (!enabled)
        {
            // HybridCache sẽ tự động dùng Redis nếu đã register AddStackExchangeRedisCache
            services.AddHybridCache();
            return services;
        }
        else
        {
            var expirationSeconds = cacheSection.GetValue<int>("ExpirationSeconds");
            var localExpirationSeconds = cacheSection.GetValue<int>("LocalCacheExpirationSeconds");
            var flags = cacheSection.GetValue<HybridCacheEntryFlags>("Flags");
            
            // HybridCache sẽ tự động dùng Redis nếu đã register AddStackExchangeRedisCache
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
