namespace VolcanionAuth.Application.Features.RoleManagement.Common;

/// <summary>
/// Represents a data transfer object containing information about a user role, including its identity, status,
/// metadata, and associated permissions.
/// </summary>
/// <param name="RoleId">The unique identifier for the role.</param>
/// <param name="Name">The display name of the role. Cannot be null.</param>
/// <param name="Description">An optional description providing additional details about the role, or null if no description is specified.</param>
/// <param name="IsActive">Indicates whether the role is currently active. Set to <see langword="true"/> if the role is enabled; otherwise,
/// <see langword="false"/>.</param>
/// <param name="CreatedAt">The date and time when the role was created, in UTC.</param>
/// <param name="UpdatedAt">The date and time when the role was last updated, in UTC, or null if the role has not been updated since creation.</param>
/// <param name="Permissions">A list of permissions associated with the role. Cannot be null; may be empty if the role has no permissions.</param>
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
/// Represents a data transfer object that defines a specific permission assigned to a role, including the resource,
/// action, and permission identifier.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission associated with the role.</param>
/// <param name="Resource">The name of the resource to which the permission applies. Cannot be null.</param>
/// <param name="Action">The action that is permitted on the specified resource. Cannot be null.</param>
/// <param name="PermissionString">A string representation of the permission, typically combining resource and action for display or comparison
/// purposes. Cannot be null.</param>
public record RolePermissionDto(
    Guid PermissionId,
    string Resource,
    string Action,
    string PermissionString
);
