using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides the Entity Framework Core configuration for the Permission entity.
/// </summary>
/// <remarks>This configuration defines the table mapping, property constraints, indexes, and relationships for
/// the Permission entity when using Entity Framework Core's model builder. It is typically used within the
/// OnModelCreating method of a DbContext to ensure the Permission entity is mapped correctly to the database
/// schema.</remarks>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    /// <summary>
    /// Configures the entity type mapping for the Permission entity.
    /// </summary>
    /// <remarks>This method sets up table mapping, property constraints, indexes, and relationships for the
    /// Permission entity using the Entity Framework Core fluent API. It should be called within the OnModelCreating
    /// method of your DbContext to ensure the Permission entity is correctly mapped to the database schema.</remarks>
    /// <param name="builder">The builder used to configure the Permission entity type.</param>
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        // Table mapping
        builder.ToTable("Permissions");
        // Primary key
        builder.HasKey(p => p.Id);
        // Property configurations
        builder.Property(p => p.Resource).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Action).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.CreatedAt).IsRequired();
        // Unique index on Resource and Action
        builder.HasIndex(p => new { p.Resource, p.Action }).IsUnique();
        // Relationship with RolePermission
        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}

/// <summary>
/// Provides the Entity Framework Core configuration for the Policy entity.
/// </summary>
/// <remarks>This class is used to configure the database schema and relationships for the Policy entity using the
/// Fluent API. It is typically used within the OnModelCreating method of a DbContext to apply entity-specific
/// configuration.</remarks>
public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    /// <summary>
    /// Configures the entity type mapping for the Policy entity.
    /// </summary>
    /// <remarks>This method is typically called by the Entity Framework Core infrastructure when building the
    /// model. It defines table mapping, property constraints, indexes, and ignored properties for the Policy
    /// entity.</remarks>
    /// <param name="builder">The builder used to configure the Policy entity type.</param>
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        // Table mapping
        builder.ToTable("Policies");
        // Primary key
        builder.HasKey(p => p.Id);
        // Property configurations
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
        // Indexes
        builder.HasIndex(p => p.Name).IsUnique();
        builder.HasIndex(p => new { p.Resource, p.Action });
        // Ignore domain events
        builder.Ignore(p => p.DomainEvents);
    }
}

/// <summary>
/// Configures the entity mapping for the UserRole type within the Entity Framework model.
/// </summary>
/// <remarks>This configuration defines table mapping, primary key, unique constraints, and required properties
/// for the UserRole entity. It is typically used in the OnModelCreating method to apply custom configuration when using
/// Entity Framework Core's code-first approach.</remarks>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    /// <summary>
    /// Configures the entity type mapping for the UserRole entity.
    /// </summary>
    /// <remarks>This method is typically called by the Entity Framework infrastructure when building the
    /// model. It configures the table name, primary key, unique index, and required properties for the UserRole
    /// entity.</remarks>
    /// <param name="builder">The builder used to configure the UserRole entity type.</param>
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Table mapping
        builder.ToTable("UserRoles");
        // Primary key
        builder.HasKey(ur => ur.Id);
        // Unique index on UserId and RoleId
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
        // Required property
        builder.Property(ur => ur.AssignedAt).IsRequired();
    }
}

/// <summary>
/// Configures the entity mapping for the RolePermission type in the Entity Framework model.
/// </summary>
/// <remarks>This configuration defines the table name, primary key, unique index, and required properties for the
/// RolePermission entity. It is typically used within the Entity Framework Core model building process to ensure the
/// RolePermission entity is mapped correctly to the underlying database schema.</remarks>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    /// <summary>
    /// Configures the entity type mapping for the RolePermission entity.
    /// </summary>
    /// <remarks>This method sets up the table name, primary key, unique index on the combination of RoleId
    /// and PermissionId, and marks the AssignedAt property as required. It is intended to be used within the Entity
    /// Framework Core model configuration process.</remarks>
    /// <param name="builder">The builder used to configure the RolePermission entity type.</param>
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        // Table mapping
        builder.ToTable("RolePermissions");
        // Primary key
        builder.HasKey(rp => rp.Id);
        // Unique index on RoleId and PermissionId
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
        // Required property
        builder.Property(rp => rp.AssignedAt).IsRequired();
    }
}

/// <summary>
/// Provides the Entity Framework Core configuration for the LoginHistory entity.
/// </summary>
/// <remarks>This configuration defines the table mapping, property constraints, and indexes for the LoginHistory
/// entity. It is typically used within the OnModelCreating method of a DbContext to apply custom model
/// configuration.</remarks>
public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    /// <summary>
    /// Configures the entity type mapping for the LoginHistory entity.
    /// </summary>
    /// <remarks>This method sets up table mapping, property constraints, and indexes for the LoginHistory
    /// entity when using Entity Framework Core. It should be called from the OnModelCreating method of your DbContext
    /// to ensure the entity is configured correctly.</remarks>
    /// <param name="builder">The builder used to configure the LoginHistory entity type.</param>
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        // Table mapping
        builder.ToTable("LoginHistories");
        // Primary key
        builder.HasKey(lh => lh.Id);
        // Property configurations
        builder.Property(lh => lh.IpAddress).HasMaxLength(45).IsRequired();
        builder.Property(lh => lh.UserAgent).HasMaxLength(500).IsRequired();
        builder.Property(lh => lh.IsSuccessful).IsRequired();
        builder.Property(lh => lh.Timestamp).IsRequired();
        builder.Property(lh => lh.FailureReason).HasMaxLength(200);
        // Indexes
        builder.HasIndex(lh => lh.UserId);
        builder.HasIndex(lh => lh.Timestamp);
    }
}

/// <summary>
/// Configures the entity mapping for the RefreshToken type in the Entity Framework model.
/// </summary>
/// <remarks>This configuration defines how the RefreshToken entity is mapped to the database, including table
/// name, keys, property constraints, and indexes. It is typically used within the Entity Framework Core model building
/// process to ensure the RefreshToken entity is correctly represented in the database schema.</remarks>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// Configures the entity type mapping for the RefreshToken entity.
    /// </summary>
    /// <remarks>This method defines the table name, primary key, property constraints, indexes, and ignored
    /// properties for the RefreshToken entity. It is typically called by the Entity Framework Core infrastructure when
    /// building the model.</remarks>
    /// <param name="builder">The builder used to configure the RefreshToken entity type.</param>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Table mapping
        builder.ToTable("RefreshTokens");
        // Primary key
        builder.HasKey(rt => rt.Id);
        // Property configurations
        builder.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
        builder.Property(rt => rt.ExpiresAt).IsRequired();
        builder.Property(rt => rt.CreatedAt).IsRequired();
        builder.Property(rt => rt.RevokedAt);
        // Indexes
        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => rt.UserId);
        // Ignore computed properties
        builder.Ignore(rt => rt.IsRevoked);
        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsActive);
    }
}

/// <summary>
/// Provides the Entity Framework Core configuration for the UserAttribute entity type.
/// </summary>
/// <remarks>This configuration defines the table mapping, primary key, property constraints, and indexes for the
/// UserAttribute entity. It is typically used within the OnModelCreating method of a DbContext to apply entity-specific
/// configuration using the Fluent API.</remarks>
public class UserAttributeConfiguration : IEntityTypeConfiguration<UserAttribute>
{
    /// <summary>
    /// Configures the entity type mapping for the UserAttribute entity.
    /// </summary>
    /// <remarks>This method is typically called by the Entity Framework Core infrastructure to apply
    /// configuration for the UserAttribute entity, including table mapping, property constraints, and
    /// indexes.</remarks>
    /// <param name="builder">The builder used to configure the UserAttribute entity type.</param>
    public void Configure(EntityTypeBuilder<UserAttribute> builder)
    {
        // Table mapping
        builder.ToTable("UserAttributes");
        // Primary key
        builder.HasKey(ua => ua.Id);
        // Property configurations
        builder.Property(ua => ua.AttributeKey).HasMaxLength(100).IsRequired();
        builder.Property(ua => ua.AttributeValue).HasMaxLength(1000).IsRequired();
        builder.Property(ua => ua.DataType).HasMaxLength(20).IsRequired();
        builder.Property(ua => ua.CreatedAt).IsRequired();
        builder.Property(ua => ua.UpdatedAt);
        // Indexes
        builder.HasIndex(ua => new { ua.UserId, ua.AttributeKey });
    }
}

/// <summary>
/// Provides the Entity Framework Core configuration for the UserRelationship entity.
/// </summary>
/// <remarks>This configuration defines the table mapping, primary key, property constraints, indexes, and
/// relationships for the UserRelationship entity. It is typically used by the Entity Framework Core infrastructure and
/// is not intended to be used directly in application code.</remarks>
public class UserRelationshipConfiguration : IEntityTypeConfiguration<UserRelationship>
{
    /// <summary>
    /// Configures the entity type mapping for the UserRelationship entity.
    /// </summary>
    /// <remarks>This method is typically called by the Entity Framework infrastructure when building the
    /// model. It defines table mapping, keys, property constraints, indexes, and relationships for the UserRelationship
    /// entity.</remarks>
    /// <param name="builder">The builder used to configure the UserRelationship entity type.</param>
    public void Configure(EntityTypeBuilder<UserRelationship> builder)
    {
        // Table mapping
        builder.ToTable("UserRelationships");
        // Primary key
        builder.HasKey(ur => ur.Id);
        // Property configurations
        builder.Property(ur => ur.RelationshipType).HasMaxLength(50).IsRequired();
        builder.Property(ur => ur.CreatedAt).IsRequired();
        // Unique index on SourceUserId, TargetUserId, and RelationshipType
        builder.HasIndex(ur => new { ur.SourceUserId, ur.TargetUserId, ur.RelationshipType }).IsUnique();
        // Relationships
        builder.HasOne(ur => ur.TargetUser)
            .WithMany()
            .HasForeignKey(ur => ur.TargetUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
