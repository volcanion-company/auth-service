using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.ToggleRoleStatus;

/// <summary>
/// Command to activate or deactivate a role.
/// </summary>
public record ToggleRoleStatusCommand(Guid RoleId, bool IsActive) : IRequest<Result<RoleDto>>;
