namespace VolcanionAuth.Application.Features.UserManagement.Common;

/// <summary>
/// Data transfer object representing a user in the system.
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    List<UserRoleDto> Roles,
    List<UserPermissionDto> Permissions
);

/// <summary>
/// Data transfer object representing a user's role assignment.
/// </summary>
public record UserRoleDto(
    Guid RoleId,
    string RoleName,
    List<UserPermissionDto> Permissions
);

/// <summary>
/// Data transfer object representing a permission.
/// </summary>
public record UserPermissionDto(
    Guid PermissionId,
    string Resource,
    string Action,
    string PermissionString
);

/// <summary>
/// Data transfer object for paginated user list results.
/// </summary>
public record UserListDto(
    List<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize
);
