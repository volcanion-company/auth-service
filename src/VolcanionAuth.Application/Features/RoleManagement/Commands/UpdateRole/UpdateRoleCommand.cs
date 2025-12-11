using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.UpdateRole;

/// <summary>
/// Command to update an existing role's information.
/// </summary>
public record UpdateRoleCommand(
    Guid RoleId,
    string? Name = null,
    string? Description = null,
    List<Guid>? PermissionIds = null
) : IRequest<Result<RoleDto>>;
