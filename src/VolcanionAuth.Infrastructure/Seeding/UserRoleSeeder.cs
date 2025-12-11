using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Seeding;

/// <summary>
/// Provides methods for seeding user-role assignments into the database.
/// </summary>
public static class UserRoleSeeder
{
    /// <summary>
    /// Seeds the database with user-role assignments if no assignments currently exist.
    /// </summary>
    /// <param name="context">The database context used to access and modify the user roles table.</param>
    /// <returns>A task that represents the asynchronous seeding operation.</returns>
    public static async Task SeedUserRolesAsync(WriteDbContext context)
    {
        // Check if any user roles already exist
        if (await context.Set<UserRole>().AnyAsync())
        {
            return;
        }

        var users = await context.Set<User>().ToListAsync();
        var roles = await context.Set<Role>().ToListAsync();

        if (!users.Any() || !roles.Any())
        {
            return;
        }

        var userRoles = new List<UserRole>();

        // Assign roles to users
        var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
        var managerRole = roles.FirstOrDefault(r => r.Name == "Manager");
        var userRole = roles.FirstOrDefault(r => r.Name == "User");
        var guestRole = roles.FirstOrDefault(r => r.Name == "Guest");
        var developerRole = roles.FirstOrDefault(r => r.Name == "Developer");
        var supportRole = roles.FirstOrDefault(r => r.Name == "Support");

        // Admin user
        var adminUser = users.FirstOrDefault(u => u.Email.Value == "admin@volcanion.com");
        if (adminUser != null && adminRole != null)
        {
            userRoles.Add(UserRole.Create(adminUser.Id, adminRole.Id));
        }

        // Manager user - Manager and User roles
        var managerUser = users.FirstOrDefault(u => u.Email.Value == "manager@volcanion.com");
        if (managerUser != null)
        {
            if (managerRole != null)
            {
                userRoles.Add(UserRole.Create(managerUser.Id, managerRole.Id));
            }
            if (userRole != null)
            {
                userRoles.Add(UserRole.Create(managerUser.Id, userRole.Id));
            }
        }

        // Regular users - User role
        var user1 = users.FirstOrDefault(u => u.Email.Value == "user1@volcanion.com");
        if (user1 != null && userRole != null)
        {
            userRoles.Add(UserRole.Create(user1.Id, userRole.Id));
        }

        var user2 = users.FirstOrDefault(u => u.Email.Value == "user2@volcanion.com");
        if (user2 != null && userRole != null)
        {
            userRoles.Add(UserRole.Create(user2.Id, userRole.Id));
        }

        // Guest user - Guest role
        var guestUser = users.FirstOrDefault(u => u.Email.Value == "guest@volcanion.com");
        if (guestUser != null && guestRole != null)
        {
            userRoles.Add(UserRole.Create(guestUser.Id, guestRole.Id));
        }

        // Developer user - Developer role
        var developerUser = users.FirstOrDefault(u => u.Email.Value == "developer@volcanion.com");
        if (developerUser != null && developerRole != null)
        {
            userRoles.Add(UserRole.Create(developerUser.Id, developerRole.Id));
        }

        // Support user - Support role
        var supportUser = users.FirstOrDefault(u => u.Email.Value == "support@volcanion.com");
        if (supportUser != null && supportRole != null)
        {
            userRoles.Add(UserRole.Create(supportUser.Id, supportRole.Id));
        }

        await context.Set<UserRole>().AddRangeAsync(userRoles);
        await context.SaveChangesAsync();
    }
}
