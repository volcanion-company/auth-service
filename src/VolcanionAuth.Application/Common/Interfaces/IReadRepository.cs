using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Read-only repository for query operations (reads from replica)
/// </summary>
public interface IReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Policy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserRelationship>> GetUserRelationshipsAsync(Guid userId, CancellationToken cancellationToken = default);
}
