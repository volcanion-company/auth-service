namespace VolcanionAuth.Application.Features.UserManagement.Common;

/// <summary>
/// Represents a data transfer object containing user account information, including identity, contact details, status,
/// timestamps, roles, and permissions.
/// </summary>
/// <param name="Id">The unique identifier for the user.</param>
/// <param name="Email">The email address associated with the user account.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="IsActive">Indicates whether the user account is currently active. Set to <see langword="true"/> if the account is active;
/// otherwise, <see langword="false"/>.</param>
/// <param name="CreatedAt">The date and time when the user account was created, in UTC.</param>
/// <param name="LastLoginAt">The date and time of the user's most recent login, in UTC, or <see langword="null"/> if the user has never logged
/// in.</param>
/// <param name="Roles">A list of roles assigned to the user, defining their access level and responsibilities.</param>
/// <param name="Permissions">A list of permissions granted to the user, specifying allowed actions within the system.</param>
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
/// Represents a data transfer object containing information about a user role, including its unique identifier, name,
/// and associated permissions.
/// </summary>
/// <param name="RoleId">The unique identifier for the user role.</param>
/// <param name="RoleName">The display name of the user role.</param>
/// <param name="Permissions">The list of permissions assigned to the user role. Cannot be null.</param>
public record UserRoleDto(
    Guid RoleId,
    string RoleName,
    List<UserPermissionDto> Permissions
);

/// <summary>
/// Represents a data transfer object that defines a user's permission for a specific resource and action.
/// </summary>
/// <param name="PermissionId">The unique identifier of the permission.</param>
/// <param name="Resource">The name of the resource to which the permission applies. Cannot be null.</param>
/// <param name="Action">The action that is permitted on the resource. Cannot be null.</param>
/// <param name="PermissionString">A string representation of the permission, typically used for display or serialization. Cannot be null.</param>
public record UserPermissionDto(
    Guid PermissionId,
    string Resource,
    string Action,
    string PermissionString
);

/// <summary>
/// Represents a paginated list of users, including user details and pagination metadata.
/// </summary>
/// <remarks>Use this record to return user data in scenarios where results are paginated, such as API responses.
/// The Users list may be empty if no users are available for the specified page.</remarks>
/// <param name="Users">The collection of user details to include in the list. Cannot be null.</param>
/// <param name="TotalCount">The total number of users available across all pages.</param>
/// <param name="Page">The current page number in the paginated result set. Must be greater than or equal to 1.</param>
/// <param name="PageSize">The maximum number of users included per page. Must be greater than 0.</param>
public record UserListDto(
    List<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize
);
