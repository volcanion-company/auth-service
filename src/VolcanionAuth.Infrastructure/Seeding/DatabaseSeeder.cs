using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Seeding;

/// <summary>
/// Provides methods for seeding all database tables with sample data.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds all database tables with sample data in the correct order.
    /// </summary>
    /// <param name="context">The database context used to access and modify the database.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="logger">The logger for recording seeding operations.</param>
    /// <returns>A task that represents the asynchronous seeding operation.</returns>
    public static async Task SeedAllAsync(
        WriteDbContext context, 
        IServiceProvider serviceProvider,
        ILogger? logger = null)
    {
        try
        {
            logger?.LogInformation("Starting database seeding...");

            // 1. Seed Roles
            logger?.LogInformation("Seeding roles...");
            await RoleSeeder.SeedRolesAsync(context);
            logger?.LogInformation("Roles seeded successfully.");

            // 2. Seed Permissions
            logger?.LogInformation("Seeding permissions...");
            await PermissionSeeder.SeedPermissionsAsync(context);
            logger?.LogInformation("Permissions seeded successfully.");

            // 3. Seed Role-Permission relationships
            logger?.LogInformation("Seeding role-permission relationships...");
            await RolePermissionSeeder.SeedRolePermissionsAsync(context);
            logger?.LogInformation("Role-permission relationships seeded successfully.");

            // 4. Seed Policies
            logger?.LogInformation("Seeding policies...");
            await PolicySeeder.SeedPoliciesAsync(context);
            logger?.LogInformation("Policies seeded successfully.");

            // 5. Seed Users
            logger?.LogInformation("Seeding users...");
            var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
            await UserSeeder.SeedUsersAsync(context, passwordHasher);
            logger?.LogInformation("Users seeded successfully.");

            // 6. Seed User-Role relationships
            logger?.LogInformation("Seeding user-role relationships...");
            await UserRoleSeeder.SeedUserRolesAsync(context);
            logger?.LogInformation("User-role relationships seeded successfully.");

            // 7. Seed User Attributes
            logger?.LogInformation("Seeding user attributes...");
            await UserAttributeSeeder.SeedUserAttributesAsync(context);
            logger?.LogInformation("User attributes seeded successfully.");

            logger?.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
