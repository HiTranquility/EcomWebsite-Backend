using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Hybrid;
using System.Diagnostics.Metrics;

namespace App.UTIL.Helpers.Cache
{
    public class HybridCacheAdapter : ICacheService
    {
        private readonly HybridCache _hybridCache;
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _prefixToKeys = new();

        private static readonly Meter CacheMeter = new("App.Cache");
        private static readonly Counter<long> HitCounter = CacheMeter.CreateCounter<long>("cache_hits");
        private static readonly Counter<long> MissCounter = CacheMeter.CreateCounter<long>("cache_misses");
        private static readonly Counter<long> SetCounter = CacheMeter.CreateCounter<long>("cache_sets");
        private static readonly Counter<long> RemoveCounter = CacheMeter.CreateCounter<long>("cache_removes");

        public HybridCacheAdapter(HybridCache hybridCache)
        {
            _hybridCache = hybridCache;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? ttl = null, string? prefix = null, CancellationToken cancellationToken = default)
        {
            var fullKey = BuildKey(key, prefix);
            var executed = false;

            var options = ttl.HasValue ? new HybridCacheEntryOptions { Expiration = ttl.Value } : null;

            var value = await _hybridCache.GetOrCreateAsync(
                fullKey,
                ct =>
                {
                    executed = true;
                    return new ValueTask<T>(factory(ct));
                },
                options,
                tags: null,
                cancellationToken: cancellationToken);

            if (executed)
            {
                MissCounter.Add(1);
                SetCounter.Add(1);
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    TrackKey(NormalizePrefix(prefix!), fullKey);
                }
            }
            else
            {
                HitCounter.Add(1);
            }

            return value;
        }

        public void Remove(string fullKey)
        {
            _hybridCache.RemoveAsync(fullKey).GetAwaiter().GetResult();
            RemoveCounter.Add(1);
        }

        public void RemoveByPrefix(string prefix)
        {
            var normalized = NormalizePrefix(prefix);
            if (_prefixToKeys.TryGetValue(normalized, out var keys))
            {
                foreach (var entry in keys.Keys)
                {
                    _hybridCache.RemoveAsync(entry).GetAwaiter().GetResult();
                    RemoveCounter.Add(1);
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


