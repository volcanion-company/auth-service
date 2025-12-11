using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.ToggleRoleStatus;

/// <summary>
/// Represents a command to update the active status of a role identified by its unique ID.
/// </summary>
/// <param name="RoleId">The unique identifier of the role whose status is to be toggled.</param>
/// <param name="IsActive">A value indicating whether the role should be set as active. Set to <see langword="true"/> to activate the role;
/// otherwise, <see langword="false"/>.</param>
public record ToggleRoleStatusCommand(Guid RoleId, bool IsActive) : IRequest<Result<RoleDto>>;
