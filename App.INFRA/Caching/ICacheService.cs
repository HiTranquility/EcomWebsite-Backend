namespace App.INFRA.Caching;

public interface ICacheService
{
    /// <summary>
    /// Gets an item from cache or sets it using the provided factory function.
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        string? prefix = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific cache entry by full key.
    /// </summary>
    Task RemoveAsync(
        string fullKey,
        CancellationToken cancellationToken = default);
    
    Task RemoveAsync(
        string fullKey,
        string? prefix,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries tracked under a given prefix.
    /// </summary>
    Task RemoveByPrefixAsync(
        string prefix,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries matching a pattern (e.g. wildcard or regex).
    /// </summary>
    void RemoveByPattern(
        string pattern,
        CancellationToken cancellationToken = default);
}

