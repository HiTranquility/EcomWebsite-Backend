using System;
using App.UTIL.Helpers.Cache;
using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Extensions;

public static class CacheServiceExtensions
{
    public static Task SetAsync(this ICacheService cache, string key, string value, TimeSpan ttl, CancellationToken ct = default)
        => cache.GetOrSetAsync(key, _ => Task.FromResult(value), ttl, prefix: null, ct);

    public static Task<string?> GetAsync(this ICacheService cache, string key, CancellationToken ct = default)
        => cache.GetOrSetAsync(key, _ => Task.FromResult<string?>(null), null, prefix: null, ct);

    public static Task RemoveAsync(this ICacheService cache, string key, CancellationToken ct = default)
        => cache.RemoveAsync(key, prefix: null, cancellationToken: ct);

    public static string BuildStructuredKey(this ICacheService cache, string prefix, Action<CacheKeyBuilder> configure)
    {
        // ArgumentNullException.ThrowIfNull(cache);
        // ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        // ArgumentNullException.ThrowIfNull(configure);

        var builder = CacheKeyBuilder.ForPrefix(prefix);
        configure(builder);
        return builder.BuildKey();
    }
}
