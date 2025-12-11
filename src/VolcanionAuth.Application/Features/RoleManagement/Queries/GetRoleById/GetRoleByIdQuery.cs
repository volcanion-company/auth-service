using VolcanionAuth.Application.Features.RoleManagement.Common;

namespace VolcanionAuth.Application.Features.RoleManagement.Queries.GetRoleById;

/// <summary>
/// Represents a query to retrieve a role by its unique identifier.
/// </summary>
/// <param name="RoleId">The unique identifier of the role to retrieve. Must be a valid, non-empty <see cref="System.Guid"/>.</param>
public record GetRoleByIdQuery(Guid RoleId) : IRequest<Result<RoleDto>>;
