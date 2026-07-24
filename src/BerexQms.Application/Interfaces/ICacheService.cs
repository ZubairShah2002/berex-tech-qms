namespace BerexQms.Application.Interfaces;

/// <summary>
/// Abstraction for distributed caching operations. Implemented in the
/// Infrastructure layer using Redis, in-memory cache, or similar providers.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cached value, or <c>null</c> if the key does not exist.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken ct);

    /// <summary>
    /// Stores a value in the cache with an optional expiration.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiry">Optional time-to-live for the cache entry.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry, CancellationToken ct);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken ct);

    /// <summary>
    /// Checks whether a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if the key exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken ct);
}
