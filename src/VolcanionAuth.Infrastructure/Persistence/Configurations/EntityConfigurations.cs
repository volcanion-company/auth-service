using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Resource).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Action).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => new { p.Resource, p.Action }).IsUnique();

        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.DomainEvents);
    }
}

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Resource).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Action).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Effect).HasMaxLength(10).IsRequired();
        builder.Property(p => p.Conditions).HasColumnType("jsonb").IsRequired();
        builder.Property(p => p.Priority).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);

        builder.HasIndex(p => p.Name).IsUnique();
        builder.HasIndex(p => new { p.Resource, p.Action });

        builder.Ignore(p => p.DomainEvents);
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => ur.Id);

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

        builder.Property(ur => ur.AssignedAt).IsRequired();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(rp => rp.Id);

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

        builder.Property(rp => rp.AssignedAt).IsRequired();
    }
}

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("LoginHistories");

        builder.HasKey(lh => lh.Id);

        builder.Property(lh => lh.IpAddress).HasMaxLength(45).IsRequired();
        builder.Property(lh => lh.UserAgent).HasMaxLength(500).IsRequired();
        builder.Property(lh => lh.IsSuccessful).IsRequired();
        builder.Property(lh => lh.Timestamp).IsRequired();
        builder.Property(lh => lh.FailureReason).HasMaxLength(200);

        builder.HasIndex(lh => lh.UserId);
        builder.HasIndex(lh => lh.Timestamp);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.CreatedAt).IsRequired();
        builder.Property(rt => rt.RevokedAt);

        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => rt.UserId);
        builder.Ignore(rt => rt.IsRevoked);
        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsActive);
    }
}

public class UserAttributeConfiguration : IEntityTypeConfiguration<UserAttribute>
{
    public void Configure(EntityTypeBuilder<UserAttribute> builder)
    {
        builder.ToTable("UserAttributes");

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.AttributeKey).HasMaxLength(100).IsRequired();
        builder.Property(ua => ua.AttributeValue).HasMaxLength(1000).IsRequired();
        builder.Property(ua => ua.DataType).HasMaxLength(20).IsRequired();
        builder.Property(ua => ua.CreatedAt).IsRequired();
        builder.Property(ua => ua.UpdatedAt);

        builder.HasIndex(ua => new { ua.UserId, ua.AttributeKey });
    }
}

public class UserRelationshipConfiguration : IEntityTypeConfiguration<UserRelationship>
{
    public void Configure(EntityTypeBuilder<UserRelationship> builder)
    {
        builder.ToTable("UserRelationships");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.RelationshipType).HasMaxLength(50).IsRequired();
        builder.Property(ur => ur.CreatedAt).IsRequired();

        builder.HasIndex(ur => new { ur.SourceUserId, ur.TargetUserId, ur.RelationshipType }).IsUnique();

        builder.HasOne(ur => ur.TargetUser)
            .WithMany()
            .HasForeignKey(ur => ur.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
