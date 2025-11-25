using Microsoft.EntityFrameworkCore;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Domain.ValueObjects;
using VolcanionAuth.Infrastructure.Persistence;

namespace VolcanionAuth.Infrastructure.Seeding;

/// <summary>
/// Provides methods for seeding sample users into the database.
/// </summary>
public static class UserSeeder
{
    /// <summary>
    /// Seeds the database with sample users if no users currently exist.
    /// </summary>
    /// <param name="context">The database context used to access and modify the users table.</param>
    /// <param name="passwordHasher">The password hasher service for hashing user passwords.</param>
    /// <returns>A task that represents the asynchronous seeding operation.</returns>
    public static async Task SeedUsersAsync(WriteDbContext context, IPasswordHasher passwordHasher)
    {
        // Check if any users already exist
        if (await context.Set<User>().AnyAsync())
        {
            return;
        }

        var users = new List<User>();

        // Admin user
        var adminEmail = Email.Create("admin@volcanion.com").Value;
        var adminPassword = Password.Create("Admin@123456").Value;
        var hashedAdminPassword = Password.CreateFromHash(passwordHasher.HashPassword(adminPassword.Hash));
        var adminFullName = FullName.Create("System", "Administrator").Value;
        var adminUser = User.Create(adminEmail, hashedAdminPassword, adminFullName).Value;
        adminUser.VerifyEmail();
        users.Add(adminUser);

        // Manager user
        var managerEmail = Email.Create("manager@volcanion.com").Value;
        var managerPassword = Password.Create("Manager@123456").Value;
        var hashedManagerPassword = Password.CreateFromHash(passwordHasher.HashPassword(managerPassword.Hash));
        var managerFullName = FullName.Create("John", "Manager").Value;
        var managerUser = User.Create(managerEmail, hashedManagerPassword, managerFullName).Value;
        managerUser.VerifyEmail();
        users.Add(managerUser);

        // Regular users
        var user1Email = Email.Create("user1@volcanion.com").Value;
        var user1Password = Password.Create("User@123456").Value;
        var hashedUser1Password = Password.CreateFromHash(passwordHasher.HashPassword(user1Password.Hash));
        var user1FullName = FullName.Create("Alice", "Johnson").Value;
        var user1 = User.Create(user1Email, hashedUser1Password, user1FullName).Value;
        user1.VerifyEmail();
        users.Add(user1);

        var user2Email = Email.Create("user2@volcanion.com").Value;
        var user2Password = Password.Create("User@123456").Value;
        var hashedUser2Password = Password.CreateFromHash(passwordHasher.HashPassword(user2Password.Hash));
        var user2FullName = FullName.Create("Bob", "Smith").Value;
        var user2 = User.Create(user2Email, hashedUser2Password, user2FullName).Value;
        user2.VerifyEmail();
        users.Add(user2);

        // Guest user
        var guestEmail = Email.Create("guest@volcanion.com").Value;
        var guestPassword = Password.Create("Guest@123456").Value;
        var hashedGuestPassword = Password.CreateFromHash(passwordHasher.HashPassword(guestPassword.Hash));
        var guestFullName = FullName.Create("Guest", "User").Value;
        var guestUser = User.Create(guestEmail, hashedGuestPassword, guestFullName).Value;
        users.Add(guestUser);

        // Developer user
        var devEmail = Email.Create("developer@volcanion.com").Value;
        var devPassword = Password.Create("Dev@123456").Value;
        var hashedDevPassword = Password.CreateFromHash(passwordHasher.HashPassword(devPassword.Hash));
        var devFullName = FullName.Create("Charlie", "Developer").Value;
        var devUser = User.Create(devEmail, hashedDevPassword, devFullName).Value;
        devUser.VerifyEmail();
        users.Add(devUser);

        // Support user
        var supportEmail = Email.Create("support@volcanion.com").Value;
        var supportPassword = Password.Create("Support@123456").Value;
        var hashedSupportPassword = Password.CreateFromHash(passwordHasher.HashPassword(supportPassword.Hash));
        var supportFullName = FullName.Create("Sarah", "Support").Value;
        var supportUser = User.Create(supportEmail, hashedSupportPassword, supportFullName).Value;
        supportUser.VerifyEmail();
        users.Add(supportUser);

        await context.Set<User>().AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
