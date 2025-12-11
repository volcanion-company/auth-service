using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides a generic repository implementation for performing CRUD operations on entities of type T using a
/// WriteDbContext.
/// </summary>
/// <remarks>This repository offers common data access methods such as retrieving, adding, updating, and removing
/// entities. It is intended to be used as a base class for more specific repositories or directly for simple scenarios.
/// All changes made through this repository are tracked by the provided WriteDbContext and require calling
/// SaveChangesAsync on the context to persist changes to the database. This class is not thread-safe.</remarks>
/// <typeparam name="T">The type of the entity managed by the repository. Must be a reference type.</typeparam>
/// <param name="context">The WriteDbContext instance used to access the underlying data store.</param>
public class Repository<T>(WriteDbContext context) : IRepository<T> where T : class
{
    /// <summary>
    /// Provides access to the writable database context for performing data operations.
    /// </summary>
    protected readonly WriteDbContext _context = context;
    /// <summary>
    /// Represents the set of entities of type <typeparamref name="T"/> in the database context.
    /// </summary>
    /// <remarks>This field provides access to query and manipulate entities of type <typeparamref name="T"/>
    /// within the underlying database context. It is typically used by repository or data access classes to perform
    /// CRUD operations.</remarks>
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

    public virtual async Task<Role?> GetRoleWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public virtual async Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
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
    /// Asynchronously finds all entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression that defines the conditions of the entities to search for.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of entities that
    /// satisfy the specified predicate. The list is empty if no entities match.</returns>
    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the first element that matches the specified predicate, or a default value if no such
    /// element is found.
    /// </summary>
    /// <param name="predicate">An expression to test each element for a condition. Only elements that satisfy this predicate are considered.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first element that matches the
    /// predicate, or the default value for type T if no such element is found.</returns>
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Asynchronously adds the specified entity to the context.
    /// </summary>
    /// <remarks>The entity is added to the context and will be inserted into the database when
    /// SaveChangesAsync is called. This method does not immediately persist changes to the database.</remarks>
    /// <param name="entity">The entity to add to the context. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Updates the specified entity in the data set.
    /// </summary>
    /// <param name="entity">The entity to update. Cannot be null.</param>
    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Removes the specified entity from the underlying data set.
    /// </summary>
    /// <remarks>The entity is marked for removal and will be deleted from the data store when changes are
    /// saved. This method does not immediately persist changes to the database.</remarks>
    /// <param name="entity">The entity to remove from the data set. Cannot be null.</param>
    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Asynchronously determines whether any entities in the set satisfy the specified condition.
    /// </summary>
    /// <param name="predicate">An expression that defines the condition to test each entity against.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if any
    /// entities satisfy the condition; otherwise, <see langword="false"/>.</returns>
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Asynchronously returns the number of entities that satisfy the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression that defines the conditions to filter the entities to be counted.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of entities that match
    /// the specified predicate.</returns>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }
}

/// <summary>
/// User repository implementation with specialized methods for User entity
/// </summary>
public class UserRepository(WriteDbContext context) : Repository<User>(context), IUserRepository
{
    private readonly new WriteDbContext _context = context;

    /// <summary>
    /// Retrieves a user by email address from the write database context
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Cannot use LINQ with Value Objects (Email.Value.ToLower() not translatable)
        // Load users and filter in-memory - this is acceptable for login operations
        // Alternative: Use stored procedure or indexed computed column
        var normalizedEmail = email.Trim().ToLower();
        var allUsers = await _context.Set<User>().ToListAsync(cancellationToken);
        return allUsers.FirstOrDefault(u => u.Email.Value.Equals(normalizedEmail, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Retrieves a user by email address with UserRoles and Role navigation properties loaded
    /// </summary>
    public async Task<User?> GetUserByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
    {
        // Load users with UserRoles only - DO NOT include LoginHistories/RefreshTokens
        // because we will be ADDING new ones, not modifying existing
        var normalizedEmail = email.Trim().ToLower();
        var allUsers = await _context.Set<User>()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync(cancellationToken);
        
        var user = allUsers.FirstOrDefault(u => u.Email.Value.Equals(normalizedEmail, StringComparison.CurrentCultureIgnoreCase));
        
        return user;
    }
}
