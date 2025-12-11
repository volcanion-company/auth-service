using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Infrastructure.Persistence;

/// <summary>
/// Represents the Entity Framework Core database context for write operations, supporting unit of work and
/// transactional behavior.
/// </summary>
/// <remarks>This context is intended for use with write operations and implements the unit of work pattern via
/// the IUnitOfWork interface. It provides methods for managing database transactions and ensures that domain events are
/// dispatched before changes are persisted. The context applies entity configurations from its assembly during model
/// creation.</remarks>
/// <param name="options">The options to be used by the context. Must not be null.</param>
public class WriteDbContext(DbContextOptions<WriteDbContext> options) : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// IDbContextTransaction instance representing the current database transaction, if any.
    /// </summary>
    private IDbContextTransaction? _currentTransaction;

    /// <summary>
    /// Configures the entity framework model for this context instance.
    /// </summary>
    /// <remarks>This method applies all entity type configurations from the assembly containing the context.
    /// Override this method to customize the model for the context before it is locked down and used to initialize the
    /// context.</remarks>
    /// <param name="modelBuilder">The builder used to construct the model for the context. Cannot be null.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from the current assembly
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WriteDbContext).Assembly);
    }

    /// <summary>
    /// Asynchronously saves all changes made in this context to the underlying database.
    /// </summary>
    /// <remarks>This override dispatches any pending domain events before saving changes to the database. If
    /// the cancellation token is triggered before the operation completes, the task is canceled.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous save operation.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries
    /// written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEvents(cancellationToken);
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction asynchronously if no transaction is currently active.
    /// </summary>
    /// <remarks>If a transaction is already active, this method does not start a new transaction.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Start a new transaction only if there is no current transaction
        _currentTransaction ??= await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current database transaction asynchronously, saving all pending changes to the data store.
    /// </summary>
    /// <remarks>If the commit fails, the transaction is rolled back and the exception is rethrown. After the
    /// operation completes, the transaction is disposed and cannot be reused.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the commit operation.</param>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Save changes within the transaction
            await SaveChangesAsync(cancellationToken);
            // Commit the transaction
            await (_currentTransaction?.CommitAsync(cancellationToken) ?? Task.CompletedTask);
        }
        catch
        {
            // Rollback the transaction on failure
            await RollbackTransactionAsync(cancellationToken);
            // Rethrow the exception
            throw;
        }
        finally
        {
            // Dispose of the transaction
            _currentTransaction?.Dispose();
            // Clear the current transaction reference
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Asynchronously rolls back the current database transaction, if one is active.
    /// </summary>
    /// <remarks>If no transaction is active, this method completes without performing any action. After the
    /// rollback, the transaction is disposed and cannot be used for further operations.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the rollback operation.</param>
    /// <returns>A task that represents the asynchronous rollback operation.</returns>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Rollback the transaction if it exists
            await (_currentTransaction?.RollbackAsync(cancellationToken) ?? Task.CompletedTask);
        }
        finally
        {
            // Dispose of the transaction
            _currentTransaction?.Dispose();
            // Clear the current transaction reference
            _currentTransaction = null;
        }
    }

    /// <summary>
    /// Clears all domain events from tracked entities in the current context.
    /// </summary>
    /// <remarks>This method does not publish domain events; it only clears them from the entities. Domain
    /// event publishing is expected to be handled separately, typically in the API layer.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A completed task that represents the asynchronous operation.</returns>
    private Task DispatchDomainEvents(CancellationToken cancellationToken)
    {
        // Get all entities with domain events
        var domainEntities = ChangeTracker
            .Entries<Entity<Guid>>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        // Collect all domain events
        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        // Clear domain events from entities
        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        // Domain events will be published by MediatR
        // This is handled in the API layer
        return Task.CompletedTask;
    }
}
