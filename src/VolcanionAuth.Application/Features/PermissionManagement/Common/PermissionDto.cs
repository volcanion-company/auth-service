namespace VolcanionAuth.Application.Features.PermissionManagement.Common;

/// <summary>
/// Represents a data transfer object that defines a specific permission, including its associated resource, action, and
/// metadata.
/// </summary>
/// <remarks>This record is commonly used to transfer permission data between application layers or services. All
/// properties are immutable and set during initialization.</remarks>
/// <param name="PermissionId">The unique identifier for the permission.</param>
/// <param name="Resource">The name of the resource to which the permission applies. Cannot be null or empty.</param>
/// <param name="Action">The action that is permitted on the specified resource. Cannot be null or empty.</param>
/// <param name="PermissionString">A string representation of the permission, typically used for storage or comparison. Cannot be null or empty.</param>
/// <param name="Description">An optional description providing additional context about the permission. Can be null.</param>
/// <param name="CreatedAt">The date and time when the permission was created, in UTC.</param>
/// <param name="RoleCount">The number of roles that are currently assigned this permission. Must be zero or greater.</param>
public record PermissionDto(
    Guid PermissionId,
    string Resource,
    string Action,
    string PermissionString,
    string? Description,
    DateTime CreatedAt,
    int RoleCount
);
