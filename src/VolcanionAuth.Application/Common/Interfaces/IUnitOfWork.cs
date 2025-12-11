namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Defines a contract for coordinating changes and managing transactions within a data context.
/// </summary>
/// <remarks>The unit of work pattern enables grouping multiple operations into a single transaction, ensuring
/// that all changes are committed or rolled back together. Implementations typically provide atomicity and consistency
/// for data operations. This interface is commonly used in repository-based architectures to encapsulate transaction
/// management and persistence logic.</remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Asynchronously saves all changes made in this context to the underlying data store.
    /// </summary>
    /// <remarks>This method will propagate any validation or concurrency errors that occur during the save
    /// operation. Multiple calls to this method may result in different numbers of affected entries depending on the
    /// current state of the context.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the save operation.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries
    /// written to the data store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation of beginning a transaction.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Commits the current transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the commit operation.</param>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously rolls back the current database transaction, undoing all changes made during the transaction.
    /// </summary>
    /// <remarks>If no transaction is active, the method completes without performing any action. This method
    /// should be called to revert changes when an error occurs or when the transaction cannot be committed.</remarks>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the rollback operation.</param>
    /// <returns>A task that represents the asynchronous rollback operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
