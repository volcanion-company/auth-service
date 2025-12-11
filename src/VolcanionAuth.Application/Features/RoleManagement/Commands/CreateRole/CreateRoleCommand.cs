using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.CreateRole;

/// <summary>
/// Command to create a new role in the system.
/// </summary>
public record CreateRoleCommand(
    string Name,
    string? Description = null,
    List<Guid>? PermissionIds = null
) : IRequest<Result<RoleDto>>;
