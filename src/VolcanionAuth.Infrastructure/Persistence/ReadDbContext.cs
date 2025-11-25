using Microsoft.EntityFrameworkCore;

namespace VolcanionAuth.Infrastructure.Persistence;

/// <summary>
/// Represents a read-only Entity Framework Core database context configured for no-tracking queries.
/// </summary>
/// <remarks>This context is intended for scenarios where entities are only read and not modified, such as
/// reporting or query-only operations. All queries executed through this context use 'NoTracking' behavior by default,
/// which can improve performance for read-heavy workloads. To configure entity mappings, apply configurations in the
/// same assembly as this context.</remarks>
public class ReadDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ReadDbContext class using the specified options. Configures the context to use
    /// no-tracking behavior for queries.
    /// </summary>
    /// <remarks>All queries executed through this context will not track returned entities, which can improve
    /// performance for read-only operations. Use this context when entity tracking is not required.</remarks>
    /// <param name="options">The options to be used by the DbContext. Must not be null.</param>
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
        // Set the query tracking behavior to NoTracking for read-only operations
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    /// <summary>
    /// Configures the entity framework model for this context by applying entity configurations from the current
    /// assembly.
    /// </summary>
    /// <remarks>This method is called by the Entity Framework runtime when the model for the context is being
    /// created. It applies all IEntityTypeConfiguration implementations found in the same assembly as the context.
    /// Override this method to customize model configuration.</remarks>
    /// <param name="modelBuilder">The builder used to construct the model for the context. Cannot be null.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from the current assembly
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly);
    }
}
