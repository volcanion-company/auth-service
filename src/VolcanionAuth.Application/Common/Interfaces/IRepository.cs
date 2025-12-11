using VolcanionAuth.Domain.Entities;
using System.Linq.Expressions;

namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Defines a generic contract for accessing and managing entities in a data store. Provides asynchronous and
/// synchronous methods for querying, adding, updating, and removing entities of a specified type.
/// </summary>
/// <remarks>This interface abstracts common data access operations, enabling decoupling of business logic from
/// data storage implementation. Implementations may interact with databases, in-memory collections, or other data
/// sources. Methods that accept a predicate use expression trees to allow flexible querying, which may be translated by
/// the underlying data provider. All asynchronous methods support cancellation via a <see cref="CancellationToken"/>
/// parameter.</remarks>
/// <typeparam name="T">The type of entity managed by the repository. Must be a reference type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity of type T if found;
    /// otherwise, null.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetRoleWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves all entities of type T from the data source.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of all entities of
    /// type T. If no entities are found, the list will be empty.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously finds all entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression that defines the conditions each entity must satisfy to be included in the result.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of entities that
    /// match the predicate. If no entities match, the list will be empty.</returns>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously returns the first element that matches the specified predicate, or a default value if no such
    /// element is found.
    /// </summary>
    /// <param name="predicate">An expression that defines the conditions of the element to search for. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first element that matches the
    /// predicate, or the default value for type <typeparamref name="T"/> if no such element is found.</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously adds the specified entity to the data store.
    /// </summary>
    /// <param name="entity">The entity to add to the data store. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the add operation.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates the specified entity in the data store.
    /// </summary>
    /// <param name="entity">The entity to update. Cannot be null.</param>
    void Update(T entity);
    /// <summary>
    /// Removes the specified entity from the collection.
    /// </summary>
    /// <param name="entity">The entity to remove from the collection. Cannot be null.</param>
    void Remove(T entity);
    /// <summary>
    /// Asynchronously determines whether any entities in the data source satisfy the specified condition.
    /// </summary>
    /// <param name="predicate">An expression that defines the condition to test against each entity.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if any
    /// entities match the condition; otherwise, <see langword="false"/>.</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously counts the number of entities that satisfy the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression that defines the conditions each entity must meet to be included in the count. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of entities matching the
    /// predicate.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for accessing and managing user entities, including retrieval by email address and support for
/// role information.
/// </summary>
/// <remarks>This interface extends <see cref="IRepository{User}"/> to provide user-specific data access
/// operations. Implementations should ensure thread safety if used in concurrent scenarios. Methods may return <see
/// langword="null"/> if no user matching the criteria is found.</remarks>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Asynchronously retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user associated with the
    /// specified email address, or null if no user is found.</returns>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves a user by email address, including the user's associated roles if found.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user with roles if found;
    /// otherwise, null.</returns>
    Task<User?> GetUserByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);
}
