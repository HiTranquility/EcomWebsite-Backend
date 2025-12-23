using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace App.INFRA.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    // 🔹 Local prefix tracking
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _prefixToKeys = new();

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    // 📦 Get or set
    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key, prefix);

        if (_cache.TryGetValue(fullKey, out T value))
        {
            return value!;
        }

        // Cache miss → tạo mới
        value = await factory(cancellationToken);
        if (value == null) return default!;

        var entryOptions = ttl.HasValue
            ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }
            : new MemoryCacheEntryOptions();

        _cache.Set(fullKey, value, entryOptions);

        if (!string.IsNullOrWhiteSpace(prefix))
            TrackKey(NormalizePrefix(prefix!), fullKey);

        return value;
    }

    // ❌ Remove (overload 1 - không prefix)
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    // ❌ Remove (overload 2 - có prefix)
    public Task RemoveAsync(string key, string? prefix = null, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key, prefix);
        _cache.Remove(fullKey);

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            var normalized = NormalizePrefix(prefix!);
            if (_prefixToKeys.TryGetValue(normalized, out var keys))
                keys.TryRemove(fullKey, out _);
        }

        return Task.CompletedTask;
    }

    // 🧹 Remove all by prefix
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizePrefix(prefix);

        if (_prefixToKeys.TryGetValue(normalized, out var keys))
        {
            foreach (var key in keys.Keys)
            {
                _cache.Remove(key);
            }
            keys.Clear();
        }

        return Task.CompletedTask;
    }

    // 🔍 Remove by pattern (optional)
    public void RemoveByPattern(string pattern, CancellationToken cancellationToken = default)
    {
        foreach (var prefix in _prefixToKeys.Keys)
        {
            if (prefix.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                _prefixToKeys.TryRemove(prefix, out _);
            }
        }
    }

    // 🧠 Helper methods
    private static void TrackKey(string normalizedPrefix, string fullKey)
    {
        var group = _prefixToKeys.GetOrAdd(normalizedPrefix, _ => new ConcurrentDictionary<string, byte>());
        group.TryAdd(fullKey, 0);
    }

    private static string NormalizePrefix(string prefix)
        => prefix.EndsWith(":") ? prefix : prefix + ":";

    private static string BuildKey(string key, string? prefix)
    {
        var resolvedPrefix = string.IsNullOrWhiteSpace(prefix) ? string.Empty : NormalizePrefix(prefix!);
        return $"{resolvedPrefix}{key}";
    }
}

