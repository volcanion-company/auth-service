using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Domain.ValueObjects.Email.Create(value).Value)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Password)
            .HasConversion(
                password => password.Hash,
                hash => Domain.ValueObjects.Password.CreateFromHash(hash))
            .HasMaxLength(500)
            .IsRequired();

        builder.OwnsOne(u => u.FullName, fn =>
        {
            fn.Property(f => f.FirstName).HasColumnName("FirstName").HasMaxLength(50).IsRequired();
            fn.Property(f => f.LastName).HasColumnName("LastName").HasMaxLength(50).IsRequired();
        });

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
