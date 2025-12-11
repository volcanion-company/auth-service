namespace VolcanionAuth.API.Services;

/// <summary>
/// Provides access to the current user's identity, roles, permissions, and custom context data within an application
/// session.
/// </summary>
/// <remarks>Implementations of this interface enable retrieval and management of user-specific information, such
/// as authentication details, authorization roles, and arbitrary custom data. This service is typically used to support
/// authorization checks, personalization, and auditing throughout the application. Thread safety and lifetime
/// management depend on the specific implementation.</remarks>
public interface IUserContextService
{
    /// <summary>
    /// Gets the unique identifier of the user associated with the current context, if available.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the email address associated with the user.
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    string? FirstName { get; }

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    string? LastName { get; }

    /// <summary>
    /// Gets the list of roles assigned to the current user.
    /// </summary>
    List<string> Roles { get; }

    /// <summary>
    /// Gets the list of permission identifiers associated with the current user or context.
    /// </summary>
    List<string> Permissions { get; }

    /// <summary>
    /// Gets a collection of custom key-value pairs associated with the current instance.
    /// </summary>
    /// <remarks>The dictionary can be used to store additional metadata or user-defined information relevant
    /// to the instance. Keys must be unique and are case-sensitive. The contents of the dictionary are not validated or
    /// interpreted by the system.</remarks>
    Dictionary<string, object> CustomData { get; }

    /// <summary>
    /// Asynchronously loads the context data for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose context is to be loaded.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    Task LoadUserContextAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Associates a custom data value with the specified key for later retrieval or processing.
    /// </summary>
    /// <param name="key">The key used to identify the custom data. Cannot be null or empty.</param>
    /// <param name="value">The value to associate with the specified key. Can be any object, including null.</param>
    void SetCustomData(string key, object value);

    /// <summary>
    /// Retrieves a custom data value associated with the specified key and attempts to cast it to the specified type.
    /// </summary>
    /// <remarks>If the value associated with <paramref name="key"/> does not exist or cannot be cast to
    /// <typeparamref name="T"/>, the method returns <see langword="null"/>. This method does not throw an exception for
    /// type mismatches.</remarks>
    /// <typeparam name="T">The type to which the custom data value should be cast. Must be compatible with the stored value.</typeparam>
    /// <param name="key">The key that identifies the custom data value to retrieve. Cannot be null or empty.</param>
    /// <returns>The custom data value cast to type <typeparamref name="T"/> if found and compatible; otherwise, <see
    /// langword="null"/>.</returns>
    T? GetCustomData<T>(string key);

    /// <summary>
    /// Determines whether the current user has the specified permission.
    /// </summary>
    /// <param name="permission">The name of the permission to check. Cannot be null or empty.</param>
    /// <returns>true if the user has the specified permission; otherwise, false.</returns>
    bool HasPermission(string permission);

    /// <summary>
    /// Determines whether the current user is assigned the specified role.
    /// </summary>
    /// <param name="role">The name of the role to check for membership. Cannot be null or empty.</param>
    /// <returns>true if the user is a member of the specified role; otherwise, false.</returns>
    bool HasRole(string role);
}
