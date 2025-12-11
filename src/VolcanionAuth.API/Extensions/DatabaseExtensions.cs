using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Infrastructure.Persistence;
using VolcanionAuth.Infrastructure.Seeding;

namespace VolcanionAuth.API.Extensions;

/// <summary>
/// Provides extension methods for initializing and seeding the application's database during startup.
/// </summary>
/// <remarks>These methods are intended to be used in ASP.NET Core application startup routines to ensure the
/// database schema is up to date and optionally populated with sample data. All methods return the original <see
/// cref="WebApplication"/> instance to support fluent configuration. Exceptions encountered during database operations
/// are logged and rethrown. These methods should be called before the application starts handling requests to ensure
/// the database is ready.</remarks>
public static class DatabaseExtensions
{
    /// <summary>
    /// Initializes the application's database by applying any pending migrations and optionally seeding sample data.
    /// </summary>
    /// <remarks>This method should be called during application startup to ensure the database schema is up
    /// to date. If <paramref name="seedData"/> is set to <see langword="true"/>, sample data will be inserted into the
    /// database, which is typically useful for development or testing scenarios. Any exceptions encountered during
    /// initialization are logged and rethrown.</remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance whose database will be initialized.</param>
    /// <param name="seedData">true to seed the database with sample data after applying migrations; otherwise, false. The default is false.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the original <see
    /// cref="WebApplication"/> instance.</returns>
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app, bool seedData = false)
    {
        // Create a new scope to retrieve scoped services
        using var scope = app.Services.CreateScope();
        // Get the service provider
        var services = scope.ServiceProvider;
        // Get the logger
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            // Log the initialization start
            logger.LogInformation("Initializing database...");

            // Get the write context for migrations
            var context = services.GetRequiredService<WriteDbContext>();

            // Apply any pending migrations
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");

            // Seed data if requested
            if (seedData)
            {
                logger.LogInformation("Seeding database with sample data...");
                await DatabaseSeeder.SeedAllAsync(context, services, logger);
                logger.LogInformation("Database seeding completed successfully.");
            }

            logger.LogInformation("Database initialization completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }

        return app;
    }

    /// <summary>
    /// Seeds the application's database with initial data and configuration required for operation.
    /// </summary>
    /// <remarks>This method should be called during application startup to ensure that the database is
    /// populated with required data before handling requests. If an error occurs during seeding, the exception is
    /// logged and rethrown.</remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance whose database will be seeded.</param>
    /// <returns>The <see cref="WebApplication"/> instance after the database has been seeded.</returns>
    public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
    {
        // Create a new scope to retrieve scoped services
        using var scope = app.Services.CreateScope();
        // Get the service provider
        var services = scope.ServiceProvider;
        // Get the logger
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            // Log the seeding start
            logger.LogInformation("Seeding database...");
            // Get the write context for seeding
            var context = services.GetRequiredService<WriteDbContext>();
            // Seed the database
            await DatabaseSeeder.SeedAllAsync(context, services, logger);
            // Log the seeding completion
            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            // Log any errors that occur during seeding
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
        // Return the application instance for chaining
        return app;
    }
}
