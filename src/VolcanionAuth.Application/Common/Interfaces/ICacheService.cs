namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Defines a contract for asynchronous cache operations, including retrieving, storing, removing, and checking the
/// existence of cached items.
/// </summary>
/// <remarks>Implementations of this interface provide mechanisms for managing cached data in a key-value store.
/// Methods support generic types and allow specifying expiration policies and cancellation tokens for operation
/// control. This interface is typically used to abstract cache access in applications, enabling flexibility in choosing
/// underlying cache providers (such as in-memory, distributed, or remote caches).</remarks>
public interface ICacheService
{
    /// <summary>
    /// Asynchronously retrieves the value associated with the specified key from the data store.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key that identifies the value to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the value associated with the
    /// specified key, or null if the key does not exist.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously sets the specified value in the cache under the given key, with an optional expiration time.
    /// </summary>
    /// <remarks>If an entry with the same key already exists, its value and expiration will be updated. The
    /// method is thread-safe and can be awaited. The actual expiration behavior may depend on the cache
    /// implementation.</remarks>
    /// <typeparam name="T">The type of the value to store in the cache.</typeparam>
    /// <param name="key">The key under which the value will be stored. Cannot be null or empty.</param>
    /// <param name="value">The value to store in the cache.</param>
    /// <param name="expiration">An optional expiration time for the cached entry. If null, the default expiration policy is applied.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous set operation.</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously removes the value associated with the specified key from the store.
    /// </summary>
    /// <param name="key">The key of the item to remove. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the remove operation.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously determines whether an entry with the specified key exists in the data store.
    /// </summary>
    /// <param name="key">The key to locate in the data store. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if an entry with
    /// the specified key exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the value associated with the specified key from the cache asynchronously, or creates and stores it using
    /// the provided factory function if it does not exist.
    /// </summary>
    /// <remarks>If the value for the specified key is not present in the cache, the factory function is
    /// invoked to generate the value, which is then stored in the cache with the specified expiration. Subsequent calls
    /// with the same key will return the cached value until it expires. This method is thread-safe and ensures that the
    /// factory function is only invoked once per cache miss, even if called concurrently.</remarks>
    /// <typeparam name="T">The type of the value to retrieve or create.</typeparam>
    /// <param name="key">The cache key used to locate the value. Cannot be null or empty.</param>
    /// <param name="factory">A function that asynchronously produces the value to store if the key is not found in the cache. Cannot be null.</param>
    /// <param name="expiration">An optional expiration time for the cached value. If null, the default cache policy is used.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the cached or newly created value of
    /// type T.</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously removes all items whose keys begin with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix to match against item keys. All items with keys starting with this value will be removed. Cannot be
    /// null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
