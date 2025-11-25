using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Events;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Role aggregate root - Authorization
/// </summary>
public class Role : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private Role() { } // EF Core

    private Role(string name, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Role> Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Role>("Role name cannot be empty.");

        if (name.Length > 100)
            return Result.Failure<Role>("Role name cannot exceed 100 characters.");

        var role = new Role(name, description);
        role.AddDomainEvent(new RoleCreatedEvent(role.Id, role.Name));
        return Result.Success(role);
    }

    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Role name cannot be empty.");

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void AddPermission(Guid permissionId)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
            return;

        var rolePermission = RolePermission.Create(Id, permissionId);
        _rolePermissions.Add(rolePermission);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public Result Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
