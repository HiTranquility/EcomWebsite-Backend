using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Caching.Hybrid;

namespace App.UTIL.Helpers.Cache
{
    public class HybridCacheService : ICacheService
    {
        private readonly HybridCache _cache;

        // 🧠 Prefix tracking (in-memory only)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _prefixKeys = new();

        // 📊 Metrics tracking
        private static readonly Meter CacheMeter = new("App.Cache");
        private static readonly Counter<long> HitCounter = CacheMeter.CreateCounter<long>("cache_hits");
        private static readonly Counter<long> MissCounter = CacheMeter.CreateCounter<long>("cache_misses");
        private static readonly Counter<long> SetCounter = CacheMeter.CreateCounter<long>("cache_sets");
        private static readonly Counter<long> RemoveCounter = CacheMeter.CreateCounter<long>("cache_removes");

        public HybridCacheService(HybridCache cache)
        {
            _cache = cache;
        }

        // 📦 Lấy từ cache hoặc tạo mới nếu chưa có
        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? ttl = null,
            string? prefix = null,
            CancellationToken cancellationToken = default)
        {
            var fullKey = BuildKey(key, prefix);
            var executed = false;

            var options = ttl.HasValue
                ? new HybridCacheEntryOptions { Expiration = ttl.Value }
                : null;

            var result = await _cache.GetOrCreateAsync(
                fullKey,
                async ct =>
                {
                    executed = true;
                    return await factory(ct);
                },
                options,
                cancellationToken: cancellationToken);

            if (executed)
            {
                MissCounter.Add(1);
                SetCounter.Add(1);

                if (!string.IsNullOrWhiteSpace(prefix))
                    TrackKey(NormalizePrefix(prefix!), fullKey);
            }
            else
            {
                HitCounter.Add(1);
            }

            return result;
        }

        // ❌ Xóa cache (overload 1 - có prefix)
        public async Task RemoveAsync(string key, string? prefix = null, CancellationToken cancellationToken = default)
        {
            var fullKey = BuildKey(key, prefix);
            await _cache.RemoveAsync(fullKey, cancellationToken);
            RemoveCounter.Add(1);

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var normalized = NormalizePrefix(prefix!);
                if (_prefixKeys.TryGetValue(normalized, out var keys))
                    keys.TryRemove(fullKey, out _);
            }
        }

        // ❌ Xóa cache (overload 2 - không prefix)
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
            RemoveCounter.Add(1);
        }

        // 🧹 Xóa toàn bộ theo prefix
        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizePrefix(prefix);

            if (_prefixKeys.TryGetValue(normalized, out var keys))
            {
                foreach (var key in keys.Keys)
                {
                    await _cache.RemoveAsync(key, cancellationToken);
                    RemoveCounter.Add(1);
                }

                keys.Clear();
            }
        }

        // 🔍 Xóa các prefix theo pattern (regex hoặc contains)
        public void RemoveByPattern(string pattern, CancellationToken cancellationToken = default)
        {
            foreach (var prefix in _prefixKeys.Keys)
            {
                if (prefix.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    _prefixKeys.TryRemove(prefix, out _);
                }
            }
        }

        // 🧩 Helper: track key vào prefix
        private static void TrackKey(string normalizedPrefix, string fullKey)
        {
            var keys = _prefixKeys.GetOrAdd(normalizedPrefix, _ => new ConcurrentDictionary<string, byte>());
            keys.TryAdd(fullKey, 0);
        }

        private static string NormalizePrefix(string prefix)
            => prefix.EndsWith(":") ? prefix : $"{prefix}:";

        private static string BuildKey(string key, string? prefix)
            => $"{(string.IsNullOrWhiteSpace(prefix) ? "" : NormalizePrefix(prefix!))}{key}";
    }
}
