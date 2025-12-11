using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Seeding;

/// <summary>
/// Provides methods for seeding default roles into the database.
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Seeds the database with predefined roles if no roles currently exist.
    /// </summary>
    /// <param name="context">The database context used to access and modify the roles table.</param>
    /// <returns>A task that represents the asynchronous seeding operation.</returns>
    public static async Task SeedRolesAsync(WriteDbContext context)
    {
        // Check if any roles already exist
        if (await context.Set<Role>().AnyAsync())
        {
            return;
        }

        var roles = new List<Role>
        {
            Role.Create("Admin", "System Administrator with full access").Value,
            Role.Create("Manager", "Department Manager with elevated privileges").Value,
            Role.Create("User", "Regular user with standard access").Value,
            Role.Create("Guest", "Guest user with limited read-only access").Value,
            Role.Create("Developer", "Developer with technical access").Value,
            Role.Create("Support", "Support staff with customer service access").Value
        };

        await context.Set<Role>().AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}
