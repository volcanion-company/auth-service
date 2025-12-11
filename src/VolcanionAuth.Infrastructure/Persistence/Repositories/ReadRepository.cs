using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides read-only data access operations for entities of type T using the specified read database context.
/// </summary>
/// <remarks>This repository is intended for querying data and does not support modification operations. It
/// provides common methods for retrieving entities and related data, such as users, permissions, policies, and
/// relationships. Thread safety depends on the underlying DbContext implementation; typically, DbContext instances are
/// not thread-safe and should not be shared across threads.</remarks>
/// <typeparam name="T">The type of the entity for which data is accessed. Must be a reference type.</typeparam>
/// <param name="context">The read database context used to access entity data.</param>
public class ReadRepository<T>(ReadDbContext context) : IReadRepository<T> where T : class
{
    /// <summary>
    /// Provides access to the read-only database context for use by derived classes.
    /// </summary>
    protected readonly ReadDbContext _context = context;
    /// <summary>
    /// Represents the set of entities of type <typeparamref name="T"/> in the database context.
    /// </summary>
    /// <remarks>This field provides access to query and manipulate entities of type <typeparamref name="T"/>
    /// using Entity Framework Core. It is typically used by repository or data access classes to perform CRUD
    /// operations.</remarks>
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    /// <summary>
    /// Asynchronously retrieves an entity with the specified identifier from the data source.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity of type T if found;
    /// otherwise, null.</returns>
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves all entities of type T from the data source.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of all entities of
    /// type T.</returns>
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve. The comparison is case-insensitive.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user with the specified email
    /// address, or <see langword="null"/> if no user is found.</returns>
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Normalize email to lowercase for case-insensitive comparison
        var normalizedEmail = email.Trim().ToLowerInvariant();
        
        // Use FromSqlRaw to query directly on the database column
        return await _context.Set<User>()
            .FromSqlRaw("SELECT * FROM \"Users\" WHERE \"Email\" = {0}", normalizedEmail)
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a user by identifier, including the user's associated roles.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user with their roles if found;
    /// otherwise, null.</returns>
    public async Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a user by identifier, including the user's roles and associated permissions.
    /// </summary>
    /// <remarks>The returned user object includes related user roles, each role, and the permissions
    /// associated with each role. This method performs a single query that eagerly loads these related
    /// entities.</remarks>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user with related roles and
    /// permissions if found; otherwise, null.</returns>
    public async Task<User?> GetUserWithPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves the set of permissions assigned to the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose permissions are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of permissions associated with the user. The list is empty if the user has no permissions.</returns>
    public async Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Use direct join query to avoid navigation property issues
        var permissions = await (
            from user in _context.Set<User>()
            join userRole in _context.Set<UserRole>() on user.Id equals userRole.UserId
            join role in _context.Set<Role>() on userRole.RoleId equals role.Id
            join rolePermission in _context.Set<RolePermission>() on role.Id equals rolePermission.RoleId
            join permission in _context.Set<Permission>() on rolePermission.PermissionId equals permission.Id
            where user.Id == userId
            select permission
        ).Distinct().ToListAsync(cancellationToken);

        return permissions;
    }

    /// <summary>
    /// Asynchronously retrieves a list of active policies, ordered by descending priority.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of active policies, ordered from highest to lowest priority. The list is empty if no active
    /// policies are found.</returns>
    public async Task<IReadOnlyList<Policy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Policy>()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Priority)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves all user relationships in which the specified user is either the source or the target.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose relationships are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of user relationships involving the specified user. The list is empty if the user has no
    /// relationships.</returns>
    public async Task<IReadOnlyList<UserRelationship>> GetUserRelationshipsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserRelationship>()
            .Where(ur => ur.SourceUserId == userId || ur.TargetUserId == userId)
            .Include(ur => ur.SourceUser)
            .Include(ur => ur.TargetUser)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a role by identifier, including the role's associated permissions.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role with its permissions if found;
    /// otherwise, null.</returns>
    public async Task<Role?> GetRoleWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves all roles, including their associated permissions.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of all roles with
    /// their permissions.</returns>
    public async Task<IReadOnlyList<Role>> GetAllRolesWithPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves all users, including their roles and associated permissions.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of all users with
    /// their roles and permissions.</returns>
    public async Task<IReadOnlyList<User>> GetAllUsersWithPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a refresh token by its token string, including the associated user.
    /// </summary>
    /// <param name="token">The refresh token string to find.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the refresh token with its user if found;
    /// otherwise, null.</returns>
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }
}
