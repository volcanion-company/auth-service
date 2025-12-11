using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.GrantPermissions;

/// <summary>
/// Represents a command to grant a set of permissions to a specific role.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to grant permissions to.</param>
/// <param name="PermissionIds">The list of permission identifiers to grant to the role.</param>
public record GrantPermissionsCommand(
    Guid RoleId,
    List<Guid> PermissionIds
) : IRequest<Result<RoleDto>>;
