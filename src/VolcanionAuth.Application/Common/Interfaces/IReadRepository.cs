using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Defines a read-only repository interface for retrieving entities and related domain data from a data source.
/// </summary>
/// <remarks>This interface provides asynchronous methods for accessing entities and related objects, such as
/// users, roles, permissions, policies, and relationships. It is intended for scenarios where data modification is not
/// required, and supports cancellation via the provided cancellation tokens. Implementations should ensure thread
/// safety and efficient data retrieval, especially for methods that may return large collections.</remarks>
/// <typeparam name="T">The type of entity to be retrieved. Must be a reference type.</typeparam>
public interface IReadRepository<T> where T : class
{
    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity of type T if found;
    /// otherwise, null.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves all entities of type <typeparamref name="T"/> from the data source.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of all entities of
    /// type <typeparamref name="T"/>. The list will be empty if no entities are found.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves a user account associated with the specified email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="User"/> if a user
    /// with the specified email exists; otherwise, <see langword="null"/>.</returns>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves a user and their associated roles by user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user and their roles if found;
    /// otherwise, <see langword="null"/>.</returns>
    Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves the specified user along with their associated permissions.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user and their permissions if
    /// found; otherwise, <see langword="null"/>.</returns>
    Task<User?> GetUserWithPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves the set of permissions assigned to the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose permissions are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of permissions
    /// assigned to the user. If the user has no permissions, the list will be empty.</returns>
    Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves a read-only list of policies that are currently active.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of active policies.
    /// If no policies are active, the list will be empty.</returns>
    Task<IReadOnlyList<Policy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves the list of relationships associated with the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose relationships are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of user
    /// relationships for the specified user. If the user has no relationships, the list will be empty.</returns>
    Task<IReadOnlyList<UserRelationship>> GetUserRelationshipsAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves the specified role along with its associated permissions.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Role"/> with its
    /// permissions if found; otherwise, <see langword="null"/>.</returns>
    Task<Role?> GetRoleWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves all roles along with their associated permissions.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of roles, each including its permissions. The list will be empty if no roles are defined.</returns>
    Task<IReadOnlyList<Role>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves all users along with their associated permissions.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of users, each including their permissions. The list will be empty if no users are found.</returns>
    Task<IReadOnlyList<User>> GetAllUsersWithPermissionsAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves the refresh token associated with the specified token value asynchronously.
    /// </summary>
    /// <param name="token">The token value used to identify the refresh token. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching <see
    /// cref="RefreshToken"/> if found; otherwise, <see langword="null"/>.</returns>
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}
