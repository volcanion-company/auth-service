namespace VolcanionAuth.Application.Features.PermissionManagement.Common;

/// <summary>
/// Data transfer object representing a permission with its details.
/// </summary>
public record PermissionDto(
    Guid PermissionId,
    string Resource,
    string Action,
    string PermissionString,
    string? Description,
    DateTime CreatedAt,
    int RoleCount
);
