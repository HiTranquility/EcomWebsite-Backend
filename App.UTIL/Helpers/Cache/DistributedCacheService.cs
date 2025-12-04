using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Collections.Concurrent;

namespace App.UTIL.Helpers.Cache
{
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        // 🧠 Local tracking: prefix → keys (chỉ lưu trong memory của process hiện tại)
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _prefixToKeys = new();

        public DistributedCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        // 📦 Lấy từ cache hoặc sinh mới nếu chưa có
        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? ttl = null,
            string? prefix = null,
            CancellationToken cancellationToken = default)
        {
            var fullKey = BuildKey(key, prefix);

            var cachedBytes = await _cache.GetAsync(fullKey, cancellationToken);
            if (cachedBytes is { Length: > 0 })
            {
                return JsonSerializer.Deserialize<T>(cachedBytes)!;
            }

            var value = await factory(cancellationToken);
            if (value == null) return default!;

            var serialized = JsonSerializer.SerializeToUtf8Bytes(value);
            var options = new DistributedCacheEntryOptions();
            if (ttl.HasValue)
                options.AbsoluteExpirationRelativeToNow = ttl;

            await _cache.SetAsync(fullKey, serialized, options, cancellationToken);

            // Track prefix → key
            if (!string.IsNullOrWhiteSpace(prefix))
                TrackKey(NormalizePrefix(prefix!), fullKey);

            return value;
        }

        // ❌ Xóa cache theo key (có prefix)
        public async Task RemoveAsync(string key, string? prefix = null, CancellationToken cancellationToken = default)
        {
            var fullKey = BuildKey(key, prefix);
            await _cache.RemoveAsync(fullKey, cancellationToken);

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                var normalized = NormalizePrefix(prefix!);
                if (_prefixToKeys.TryGetValue(normalized, out var keys))
                    keys.TryRemove(fullKey, out _);
            }
        }

        // ❌ Overload: Xóa cache chỉ với key (không prefix)
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }

        // 🚮 Xóa toàn bộ cache theo prefix group
        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizePrefix(prefix);
            if (_prefixToKeys.TryGetValue(normalized, out var keys))
            {
                foreach (var key in keys.Keys)
                {
                    await _cache.RemoveAsync(key, cancellationToken);
                }
                keys.Clear();
            }
        }

        // 🔧 Helper: Lưu key vào nhóm prefix
        private static void TrackKey(string normalizedPrefix, string fullKey)
        {
            var group = _prefixToKeys.GetOrAdd(normalizedPrefix, _ => new ConcurrentDictionary<string, byte>());
            group.TryAdd(fullKey, 0);
        }

        public void RemoveByPattern(string pattern, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        // ✅ Đảm bảo prefix có dấu ":"
        private static string NormalizePrefix(string prefix)
            => prefix.EndsWith(":") ? prefix : prefix + ":";

        // ✅ Xây key đầy đủ để lưu
        private static string BuildKey(string key, string? prefix)
        {
            var resolvedPrefix = string.IsNullOrWhiteSpace(prefix) ? string.Empty : NormalizePrefix(prefix!);
            return $"{resolvedPrefix}{key}";
        }
    }
}
