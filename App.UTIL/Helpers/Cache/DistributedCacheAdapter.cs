using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Collections.Concurrent;

namespace App.UTIL.Helpers.Cache
{
    public class DistributedCacheAdapter : ICacheService
    {
        private readonly IDistributedCache _cache;
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _prefixToKeys = new();

        public DistributedCacheAdapter(IDistributedCache cache)
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

            var cachedBytes = await _cache.GetAsync(fullKey, cancellationToken);
            if (cachedBytes != null)
            {
                return JsonSerializer.Deserialize<T>(cachedBytes)!;
            }

            var value = await factory(cancellationToken);

            var serialized = JsonSerializer.SerializeToUtf8Bytes(value);
            var options = new DistributedCacheEntryOptions();
            if (ttl.HasValue) options.AbsoluteExpirationRelativeToNow = ttl;

            await _cache.SetAsync(fullKey, serialized, options, cancellationToken);

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
