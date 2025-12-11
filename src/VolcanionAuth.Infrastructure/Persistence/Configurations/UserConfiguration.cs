using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures the entity mapping for the User type in the Entity Framework Core model.
/// </summary>
/// <remarks>This configuration defines table mappings, property conversions, constraints, and relationships for
/// the User entity. It should be registered with the Entity Framework Core model builder to ensure the User entity is
/// correctly mapped to the database schema.</remarks>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures the entity type mapping for the User entity in the Entity Framework model builder.
    /// </summary>
    /// <remarks>This method defines table mapping, property configurations, indexes, relationships, and value
    /// conversions for the User entity. It should be called within the Entity Framework model configuration process,
    /// typically in the OnModelCreating method of your DbContext.</remarks>
    /// <param name="builder">The builder used to configure the User entity type.</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table mapping
        builder.ToTable("Users");
        // Primary key
        builder.HasKey(u => u.Id);

        // Email property configurations
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Domain.ValueObjects.Email.Create(value).Value)
            .HasMaxLength(255)
            .IsRequired();
        // Unique index on Email
        builder.HasIndex(u => u.Email).IsUnique();

        // Password property with conversion
        builder.Property(u => u.Password)
            .HasConversion(
                password => password.Hash,
                hash => Domain.ValueObjects.Password.CreateFromHash(hash))
            .HasMaxLength(500)
            .IsRequired();

        // FullName as owned entity
        builder.OwnsOne(u => u.FullName, fn =>
        {
            // Configure FirstName properties
            fn.Property(f => f.FirstName).HasColumnName("FirstName").HasMaxLength(50).IsRequired();
            // Configure LastName properties
            fn.Property(f => f.LastName).HasColumnName("LastName").HasMaxLength(50).IsRequired();
        });
        // Other properties
        builder.Property(u => u.IsEmailVerified).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.IsLocked).IsRequired();
        builder.Property(u => u.FailedLoginAttempts).IsRequired();
        builder.Property(u => u.LastLoginAt);
        builder.Property(u => u.LockedUntil);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);

        // Relationships
        builder.HasMany(u => u.LoginHistories)
            .WithOne(lh => lh.User)
            .HasForeignKey(lh => lh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserAttributes)
            .WithOne(ua => ua.User)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Relationships)
            .WithOne(ur => ur.SourceUser)
            .HasForeignKey(ur => ur.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);
    }
}
