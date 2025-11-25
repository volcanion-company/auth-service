using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Infrastructure.Persistence;
using VolcanionAuth.Infrastructure.Seeding;

namespace VolcanionAuth.API.Extensions;

/// <summary>
/// Provides extension methods for database initialization and seeding.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Ensures the database is created and migrated, and optionally seeds it with sample data.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="seedData">Whether to seed the database with sample data.</param>
    /// <returns>The same web application instance for method chaining.</returns>
    public static async Task<WebApplication> InitializeDatabaseAsync(
        this WebApplication app, 
        bool seedData = false)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
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
    /// Seeds the database with sample data without running migrations.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <returns>The same web application instance for method chaining.</returns>
    public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Seeding database...");

            var context = services.GetRequiredService<WriteDbContext>();
            await DatabaseSeeder.SeedAllAsync(context, services, logger);

            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }

        return app;
    }
}
