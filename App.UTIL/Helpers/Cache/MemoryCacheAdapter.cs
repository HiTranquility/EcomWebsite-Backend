using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace App.UTIL.Helpers.Cache
{
    public class MemoryCacheAdapter : ICacheService
    {
        private readonly IMemoryCache _cache;
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _prefixToKeys = new();

        public MemoryCacheAdapter(IMemoryCache cache)
        {
            _cache = cache;
        }

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
            var entryOptions = ttl.HasValue ? new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            } : new MemoryCacheEntryOptions();

            _cache.Set(fullKey, value, entryOptions);

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                TrackKey(NormalizePrefix(prefix!), fullKey);
            }

            return value;
        }

        public void Remove(string fullKey)
        {
            _cache.Remove(fullKey);
        }

        public void RemoveByPrefix(string prefix)
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
        }

        private static void TrackKey(string normalizedPrefix, string fullKey)
        {
            var bag = _prefixToKeys.GetOrAdd(normalizedPrefix, _ => new ConcurrentDictionary<string, byte>());
            bag.TryAdd(fullKey, 0);
        }

        private static string NormalizePrefix(string prefix)
        {
            return prefix.EndsWith(":") ? prefix : prefix + ":";
        }

        private static string BuildKey(string key, string? prefix)
        {
            var resolvedPrefix = string.IsNullOrWhiteSpace(prefix) ? string.Empty : NormalizePrefix(prefix!);
            return $"{resolvedPrefix}{key}";
        }
    }
}
