using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Seeding;

/// <summary>
/// Provides methods for seeding sample user attributes into the database.
/// </summary>
public static class UserAttributeSeeder
{
    /// <summary>
    /// Seeds the database with sample user attributes if no attributes currently exist.
    /// </summary>
    /// <param name="context">The database context used to access and modify the user attributes table.</param>
    /// <returns>A task that represents the asynchronous seeding operation.</returns>
    public static async Task SeedUserAttributesAsync(WriteDbContext context)
    {
        // Check if any user attributes already exist
        if (await context.Set<UserAttribute>().AnyAsync())
        {
            return;
        }

        var users = await context.Set<User>().ToListAsync();

        if (!users.Any())
        {
            return;
        }

        var userAttributes = new List<UserAttribute>();

        // Admin user attributes
        var adminUser = users.FirstOrDefault(u => u.Email.Value == "admin@volcanion.com");
        if (adminUser != null)
        {
            userAttributes.Add(UserAttribute.Create(adminUser.Id, "department", "IT"));
            userAttributes.Add(UserAttribute.Create(adminUser.Id, "location", "HQ"));
            userAttributes.Add(UserAttribute.Create(adminUser.Id, "level", "senior"));
        }

        // Manager user attributes
        var managerUser = users.FirstOrDefault(u => u.Email.Value == "manager@volcanion.com");
        if (managerUser != null)
        {
            userAttributes.Add(UserAttribute.Create(managerUser.Id, "department", "Sales"));
            userAttributes.Add(UserAttribute.Create(managerUser.Id, "location", "HQ"));
            userAttributes.Add(UserAttribute.Create(managerUser.Id, "level", "manager"));
        }

        // User1 attributes
        var user1 = users.FirstOrDefault(u => u.Email.Value == "user1@volcanion.com");
        if (user1 != null)
        {
            userAttributes.Add(UserAttribute.Create(user1.Id, "department", "Sales"));
            userAttributes.Add(UserAttribute.Create(user1.Id, "location", "Branch-A"));
            userAttributes.Add(UserAttribute.Create(user1.Id, "level", "junior"));
        }

        // User2 attributes
        var user2 = users.FirstOrDefault(u => u.Email.Value == "user2@volcanion.com");
        if (user2 != null)
        {
            userAttributes.Add(UserAttribute.Create(user2.Id, "department", "Marketing"));
            userAttributes.Add(UserAttribute.Create(user2.Id, "location", "Branch-B"));
            userAttributes.Add(UserAttribute.Create(user2.Id, "level", "mid"));
        }

        // Developer user attributes
        var developerUser = users.FirstOrDefault(u => u.Email.Value == "developer@volcanion.com");
        if (developerUser != null)
        {
            userAttributes.Add(UserAttribute.Create(developerUser.Id, "department", "IT"));
            userAttributes.Add(UserAttribute.Create(developerUser.Id, "location", "Remote"));
            userAttributes.Add(UserAttribute.Create(developerUser.Id, "level", "senior"));
        }

        // Support user attributes
        var supportUser = users.FirstOrDefault(u => u.Email.Value == "support@volcanion.com");
        if (supportUser != null)
        {
            userAttributes.Add(UserAttribute.Create(supportUser.Id, "department", "Support"));
            userAttributes.Add(UserAttribute.Create(supportUser.Id, "location", "HQ"));
            userAttributes.Add(UserAttribute.Create(supportUser.Id, "level", "mid"));
        }

        await context.Set<UserAttribute>().AddRangeAsync(userAttributes);
        await context.SaveChangesAsync();
    }
}
