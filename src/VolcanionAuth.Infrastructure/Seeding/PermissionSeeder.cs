using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Seeding;

/// <summary>
/// Provides methods for seeding default permissions into the database.
/// </summary>
public static class PermissionSeeder
{
    /// <summary>
    /// Seeds the database with predefined permissions if no permissions currently exist.
    /// </summary>
    /// <param name="context">The database context used to access and modify the permissions table.</param>
    /// <returns>A task that represents the asynchronous seeding operation.</returns>
    public static async Task SeedPermissionsAsync(WriteDbContext context)
    {
        // Check if any permissions already exist
        if (await context.Set<Permission>().AnyAsync())
        {
            return;
        }

        var permissions = new List<Permission>
        {
            // User permissions
            Permission.Create("users", "read", "View user information").Value,
            Permission.Create("users", "write", "Create and update users").Value,
            Permission.Create("users", "delete", "Delete users").Value,
            Permission.Create("users", "manage", "Full user management access").Value,

            // Role permissions
            Permission.Create("roles", "read", "View roles").Value,
            Permission.Create("roles", "write", "Create and update roles").Value,
            Permission.Create("roles", "delete", "Delete roles").Value,
            Permission.Create("roles", "assign", "Assign roles to users").Value,

            // Permission permissions
            Permission.Create("permissions", "read", "View permissions").Value,
            Permission.Create("permissions", "write", "Create and update permissions").Value,
            Permission.Create("permissions", "delete", "Delete permissions").Value,
            Permission.Create("permissions", "assign", "Assign permissions to roles").Value,

            // Policy permissions
            Permission.Create("policies", "read", "View policies").Value,
            Permission.Create("policies", "write", "Create and update policies").Value,
            Permission.Create("policies", "delete", "Delete policies").Value,
            Permission.Create("policies", "evaluate", "Evaluate policy conditions").Value,

            // Document permissions
            Permission.Create("documents", "read", "View documents").Value,
            Permission.Create("documents", "write", "Create and update documents").Value,
            Permission.Create("documents", "delete", "Delete documents").Value,
            Permission.Create("documents", "edit", "Edit existing documents").Value,
            Permission.Create("documents", "share", "Share documents with others").Value,

            // Order permissions
            Permission.Create("orders", "read", "View orders").Value,
            Permission.Create("orders", "write", "Create and update orders").Value,
            Permission.Create("orders", "delete", "Delete orders").Value,
            Permission.Create("orders", "approve", "Approve orders").Value,
            Permission.Create("orders", "cancel", "Cancel orders").Value,

            // Support permissions
            Permission.Create("support", "access", "Access support system").Value,
            Permission.Create("support", "read", "View support tickets").Value,
            Permission.Create("support", "write", "Create and update support tickets").Value,
            Permission.Create("support", "resolve", "Resolve support tickets").Value,

            // System permissions
            Permission.Create("system", "read", "View system information").Value,
            Permission.Create("system", "write", "Modify system settings").Value,
            Permission.Create("system", "admin", "Full system administration access").Value,

            // Report permissions
            Permission.Create("reports", "read", "View reports").Value,
            Permission.Create("reports", "write", "Create and modify reports").Value,
            Permission.Create("reports", "export", "Export reports").Value
        };

        await context.Set<Permission>().AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }
}
