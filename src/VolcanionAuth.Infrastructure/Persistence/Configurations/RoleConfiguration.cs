using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Provides the Entity Framework Core configuration for the Role entity.
/// </summary>
/// <remarks>This class configures the database schema for the Role entity, including table mapping, property
/// constraints, indexes, and relationships. It is typically used by the Entity Framework Core infrastructure and is not
/// intended to be used directly in application code.</remarks>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    /// <summary>
    /// Configures the entity type for the Role model using the specified builder.
    /// </summary>
    /// <remarks>This method is typically called by the Entity Framework Core infrastructure when building the
    /// model. It defines table mapping, property constraints, indexes, and relationships for the Role entity.</remarks>
    /// <param name="builder">The builder used to configure the Role entity type and its relationships.</param>
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Table mapping
        builder.ToTable("Roles");
        // Primary key
        builder.HasKey(r => r.Id);
        // Property configurations
        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();
        // Unique index on Name
        builder.HasIndex(r => r.Name).IsUnique();
        // Optional Description with max length
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.IsActive).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt);
        // Relationship with RolePermissions
        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        // Relationship with UserRoles
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);
    }
}
