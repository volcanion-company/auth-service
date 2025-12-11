using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.CreateRole;

/// <summary>
/// Represents a command to create a new role with the specified name, description, and associated permissions.
/// </summary>
/// <remarks>Use this command to create a role and assign permissions in a single operation. The role name must be
/// unique within the system.</remarks>
/// <param name="Name">The name of the role to be created. Cannot be null or empty.</param>
/// <param name="Description">An optional description for the role. May be null if no description is provided.</param>
/// <param name="PermissionIds">A list of permission identifiers to associate with the role. May be null or empty if the role has no permissions.</param>
public record CreateRoleCommand(
    string Name,
    string? Description = null,
    List<Guid>? PermissionIds = null
) : IRequest<Result<RoleDto>>;
