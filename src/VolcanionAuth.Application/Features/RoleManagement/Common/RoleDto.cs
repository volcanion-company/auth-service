namespace VolcanionAuth.Application.Features.RoleManagement.Common;

/// <summary>
/// Data transfer object representing a role with its details and permissions.
/// </summary>
public record RoleDto(
    Guid RoleId,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<RolePermissionDto> Permissions
);

/// <summary>
/// Data transfer object representing a permission assigned to a role.
/// </summary>
public record RolePermissionDto(
    Guid PermissionId,
    string Resource,
    string Action,
    string PermissionString
);
