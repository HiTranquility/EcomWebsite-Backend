namespace App.UTIL.Helpers.Cache;

public interface ICacheService
{
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        string? prefix = null,
        CancellationToken cancellationToken = default);

    void Remove(string fullKey);

    void RemoveByPrefix(string prefix);
}