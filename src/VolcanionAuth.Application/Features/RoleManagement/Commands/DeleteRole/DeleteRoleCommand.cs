namespace VolcanionAuth.Application.Features.RoleManagement.Commands.DeleteRole;

/// <summary>
/// Command to delete a role from the system.
/// </summary>
public record DeleteRoleCommand(Guid RoleId) : IRequest<Result>;
