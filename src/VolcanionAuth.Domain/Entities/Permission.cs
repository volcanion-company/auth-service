using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Permission entity for granular access control
/// </summary>
public class Permission : AggregateRoot<Guid>
{
    public string Resource { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() { } // EF Core

    private Permission(string resource, string action, string? description)
    {
        Id = Guid.NewGuid();
        Resource = resource;
        Action = action;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Permission> Create(string resource, string action, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(resource))
            return Result.Failure<Permission>("Resource cannot be empty.");

        if (string.IsNullOrWhiteSpace(action))
            return Result.Failure<Permission>("Action cannot be empty.");

        var permission = new Permission(resource, action, description);
        return Result.Success(permission);
    }

    public string GetPermissionString() => $"{Resource}:{Action}";
}
