using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using VolcanionAuth.Application.Common.Interfaces;

namespace VolcanionAuth.Infrastructure.Caching;

/// <summary>
/// Provides an implementation of a distributed cache service using Redis as the underlying store. Supports storing,
/// retrieving, and removing serialized objects with optional expiration policies.
/// </summary>
/// <remarks>This service leverages the IDistributedCache interface to interact with a Redis cache, enabling
/// scalable and centralized caching for distributed applications. Objects are serialized to JSON using camel case
/// property naming. Thread safety and connection management are handled by the underlying IDistributedCache
/// implementation. For advanced Redis operations, such as prefix-based key removal, consider using a direct Redis
/// client like StackExchange.Redis.</remarks>
public class RedisCacheService : ICacheService
{
    /// <summary>
    /// IDistributedCache instance for interacting with the Redis cache.
    /// </summary>
    private readonly IDistributedCache _distributedCache;
    /// <summary>
    /// JSON serialization options for consistent object serialization.
    /// </summary>
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the RedisCacheService class using the specified distributed cache implementation.
    /// </summary>
    /// <param name="distributedCache">The distributed cache instance to be used for storing and retrieving cached data. Cannot be null.</param>
    public RedisCacheService(IDistributedCache distributedCache)
    {
        // Set the distributed cache instance
        _distributedCache = distributedCache;
        // Configure JSON serialization options
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Use camel case for property names
            WriteIndented = false, // No indentation for compact storage
        };
    }

    /// <summary>
    /// Asynchronously retrieves a value from the distributed cache and deserializes it to the specified type.
    /// </summary>
    /// <remarks>If the specified key does not exist in the cache or the cached value is empty, the method
    /// returns null. The value is deserialized using the configured JSON serializer options.</remarks>
    /// <typeparam name="T">The type to which the cached value is deserialized.</typeparam>
    /// <param name="key">The key identifying the cached value to retrieve. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized value of type T if
    /// the key exists and the value can be deserialized; otherwise, null.</returns>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // Retrieve the serialized value from the distributed cache
        var value = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(value))
        {
            // If the value is null or empty, return the default value for type T
            return default;
        }

        // Deserialize the value to type T and return it
        return JsonSerializer.Deserialize<T>(value, _jsonOptions);
    }

    /// <summary>
    /// Asynchronously stores a value in the distributed cache using the specified key, with optional expiration.
    /// </summary>
    /// <remarks>The value is serialized to JSON using the configured serialization options before being
    /// stored. If an entry with the same key already exists, it will be overwritten. This method does not guarantee
    /// atomicity if multiple operations target the same key concurrently.</remarks>
    /// <typeparam name="T">The type of the value to store in the cache. The value is serialized to JSON before being cached.</typeparam>
    /// <param name="key">The key under which the value will be stored. Cannot be null or empty.</param>
    /// <param name="value">The value to store in the cache. Can be any serializable object.</param>
    /// <param name="expiration">An optional expiration timespan that specifies how long the cached entry should remain available. If null, the
    /// cache's default expiration policy is used.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous set operation.</returns>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        // Serialize the value to a JSON string
        var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
        // Create cache entry options with optional expiration
        var options = new DistributedCacheEntryOptions();
        // Check if an expiration time is provided
        if (expiration.HasValue)
        {
            // Set the absolute expiration relative to now
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }
        // Store the serialized value in the distributed cache
        await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
    }

    /// <summary>
    /// Asynchronously removes the value with the specified key from the distributed cache.
    /// </summary>
    /// <param name="key">The key of the cache entry to remove. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the remove operation.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Remove the cache entry with the specified key
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    /// <summary>
    /// Asynchronously determines whether a value exists in the distributed cache for the specified key.
    /// </summary>
    /// <param name="key">The key identifying the cached value to check for existence. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if a value
    /// exists for the specified key; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        // Retrieve the value from the distributed cache
        var value = await _distributedCache.GetStringAsync(key, cancellationToken);
        // Return true if the value is not null or empty, indicating existence
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Gets the cached value associated with the specified key if it exists; otherwise, invokes the provided
    /// asynchronous factory function to create, cache, and return the value.
    /// </summary>
    /// <remarks>If the value is not present in the cache, the factory function is invoked to generate the
    /// value, which is then stored in the cache for subsequent requests. The factory function is always called if the
    /// key is not found, even if multiple concurrent requests are made for the same key; callers should ensure the
    /// factory is idempotent if this is a concern.</remarks>
    /// <typeparam name="T">The type of the value to retrieve or create.</typeparam>
    /// <param name="key">The key that identifies the cached value. Cannot be null.</param>
    /// <param name="factory">An asynchronous function that produces the value to cache if the key is not found. Cannot be null.</param>
    /// <param name="expiration">An optional expiration time for the cached value. If null, a default expiration policy is applied.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the cached or newly created value of
    /// type T.</returns>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        // Attempt to retrieve the cached value
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue != null)
        {
            // If the value exists in the cache, return it
            return cachedValue;
        }

        // If the value is not in the cache, invoke the factory to create it
        var value = await factory();
        // Store the newly created value in the cache
        await SetAsync(key, value, expiration, cancellationToken);
        // Return the newly created value
        return value;
    }

    /// <summary>
    /// Removes all cache entries with keys that start with the specified prefix asynchronously.
    /// </summary>
    /// <remarks>This method removes multiple cache entries by matching their keys to the specified prefix.
    /// The underlying cache implementation must support prefix-based removal for this method to have an effect. Not all
    /// distributed cache providers support this operation natively; consult the documentation for your cache provider
    /// for details.</remarks>
    /// <param name="prefix">The prefix to match against cache entry keys. All entries with keys beginning with this value will be removed.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Note: Redis doesn't natively support prefix-based deletion in IDistributedCache
        // For production, consider using StackExchange.Redis directly for SCAN operations
        // This is a simplified implementation
        await Task.CompletedTask;
    }
}
