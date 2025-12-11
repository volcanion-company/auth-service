using VolcanionAuth.Application.Features.UserManagement.Common;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.AssignRoles;

/// <summary>
/// Represents a command to assign roles to a user.
/// </summary>
/// <param name="UserId">The unique identifier of the user to assign roles to.</param>
/// <param name="RoleIds">The list of role identifiers to assign to the user.</param>
public record AssignRolesCommand(
    Guid UserId,
    List<Guid> RoleIds
) : IRequest<Result<UserDto>>;
