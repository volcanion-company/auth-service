using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Seeding;

/// <summary>
/// Provides methods for seeding role-permission relationships into the database.
/// </summary>
public static class RolePermissionSeeder
{
    /// <summary>
    /// Seeds the database with role-permission assignments if no assignments currently exist.
    /// </summary>
    /// <param name="context">The database context used to access and modify the role permissions table.</param>
    /// <returns>A task that represents the asynchronous seeding operation.</returns>
    public static async Task SeedRolePermissionsAsync(WriteDbContext context)
    {
        // Check if any role permissions already exist
        if (await context.Set<RolePermission>().AnyAsync())
        {
            return;
        }

        var roles = await context.Set<Role>().ToListAsync();
        var permissions = await context.Set<Permission>().ToListAsync();

        if (!roles.Any() || !permissions.Any())
        {
            return;
        }

        var rolePermissions = new List<RolePermission>();

        // Admin role - All permissions
        var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
        if (adminRole != null)
        {
            foreach (var permission in permissions)
            {
                rolePermissions.Add(RolePermission.Create(adminRole.Id, permission.Id));
            }
        }

        // Manager role - Most permissions except system admin
        var managerRole = roles.FirstOrDefault(r => r.Name == "Manager");
        if (managerRole != null)
        {
            var managerPermissions = permissions.Where(p =>
                p.Resource != "system" ||
                (p.Resource == "system" && p.Action == "read")
            );

            foreach (var permission in managerPermissions)
            {
                rolePermissions.Add(RolePermission.Create(managerRole.Id, permission.Id));
            }
        }

        // User role - Standard permissions
        var userRole = roles.FirstOrDefault(r => r.Name == "User");
        if (userRole != null)
        {
            var userPermissions = permissions.Where(p =>
                (p.Resource == "users" && p.Action == "read") ||
                (p.Resource == "documents" && p.Action != "delete") ||
                (p.Resource == "orders" && (p.Action == "read" || p.Action == "write")) ||
                (p.Resource == "reports" && p.Action == "read") ||
                (p.Resource == "support" && p.Action != "resolve")
            );

            foreach (var permission in userPermissions)
            {
                rolePermissions.Add(RolePermission.Create(userRole.Id, permission.Id));
            }
        }

        // Guest role - Read-only permissions
        var guestRole = roles.FirstOrDefault(r => r.Name == "Guest");
        if (guestRole != null)
        {
            var guestPermissions = permissions.Where(p => p.Action == "read");

            foreach (var permission in guestPermissions)
            {
                rolePermissions.Add(RolePermission.Create(guestRole.Id, permission.Id));
            }
        }

        // Developer role - Technical permissions
        var developerRole = roles.FirstOrDefault(r => r.Name == "Developer");
        if (developerRole != null)
        {
            var developerPermissions = permissions.Where(p =>
                p.Resource == "system" ||
                p.Resource == "reports" ||
                (p.Resource == "documents" && p.Action == "read") ||
                (p.Resource == "users" && p.Action == "read")
            );

            foreach (var permission in developerPermissions)
            {
                rolePermissions.Add(RolePermission.Create(developerRole.Id, permission.Id));
            }
        }

        // Support role - Support-related permissions
        var supportRole = roles.FirstOrDefault(r => r.Name == "Support");
        if (supportRole != null)
        {
            var supportPermissions = permissions.Where(p =>
                p.Resource == "support" ||
                (p.Resource == "users" && p.Action == "read") ||
                (p.Resource == "orders" && p.Action == "read")
            );

            foreach (var permission in supportPermissions)
            {
                rolePermissions.Add(RolePermission.Create(supportRole.Id, permission.Id));
            }
        }

        await context.Set<RolePermission>().AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();
    }
}
