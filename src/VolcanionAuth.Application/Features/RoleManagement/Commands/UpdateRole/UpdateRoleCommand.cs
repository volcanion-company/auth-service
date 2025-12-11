using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.UpdateRole;

/// <summary>
/// Represents a request to update an existing role, including its name, description, and associated permissions.
/// </summary>
/// <remarks>Any parameters set to <see langword="null"/> will not modify the corresponding property of the role.
/// This command is typically used in administrative scenarios to manage role definitions and permissions.</remarks>
/// <param name="RoleId">The unique identifier of the role to update. Must reference an existing role.</param>
/// <param name="Name">The new name for the role, or <see langword="null"/> to leave the name unchanged.</param>
/// <param name="Description">The new description for the role, or <see langword="null"/> to leave the description unchanged.</param>
/// <param name="PermissionIds">A list of permission identifiers to assign to the role, or <see langword="null"/> to retain the current permissions.</param>
public record UpdateRoleCommand(
    Guid RoleId,
    string? Name = null,
    string? Description = null,
    List<Guid>? PermissionIds = null
) : IRequest<Result<RoleDto>>;
