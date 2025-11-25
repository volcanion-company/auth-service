using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Queries.GetRoleById;

/// <summary>
/// Query to retrieve detailed information about a specific role.
/// </summary>
public record GetRoleByIdQuery(Guid RoleId) : IRequest<Result<RoleDto>>;
