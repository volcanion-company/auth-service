namespace VolcanionAuth.Application.Features.RoleManagement.Commands.DeleteRole;

/// <summary>
/// Represents a request to delete a role identified by its unique identifier.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to be deleted. Must correspond to an existing role.</param>
public record DeleteRoleCommand(Guid RoleId) : IRequest<Result>;
