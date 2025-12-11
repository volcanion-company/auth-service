using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PermissionManagement.Queries.GetPermissionsGroupedByResource;

/// <summary>
/// Handles queries to retrieve permissions grouped by resource, returning a list of permission groups for each
/// resource.
/// </summary>
/// <remarks>This handler aggregates all permissions by their associated resource and orders both resources and
/// actions alphabetically. The result provides a structured view of permissions, grouped for easier consumption in
/// scenarios such as access control management or UI display.</remarks>
/// <param name="permissionRepository">The repository used to access and retrieve permission entities from the data store.</param>
public class GetPermissionsGroupedByResourceQueryHandler(IReadRepository<Permission> permissionRepository) : IRequestHandler<GetPermissionsGroupedByResourceQuery, Result<List<PermissionsByResourceDto>>>
{
    /// <summary>
    /// Handles the query to retrieve all permissions grouped by their associated resource.
    /// </summary>
    /// <param name="request">The query containing criteria for grouping permissions by resource.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing a list of permission groups, where each group represents permissions for a specific
    /// resource. The list is empty if no permissions are found.</returns>
    public async Task<Result<List<PermissionsByResourceDto>>> Handle(GetPermissionsGroupedByResourceQuery request, CancellationToken cancellationToken)
    {
        // Retrieve all permissions from the repository
        var allPermissions = await permissionRepository.GetAllAsync(cancellationToken);
        // Group permissions by resource and order them
        var groupedPermissions = allPermissions
            .GroupBy(p => p.Resource)
            .OrderBy(g => g.Key)
            .Select(g => new PermissionsByResourceDto(
                g.Key,
                [.. g.OrderBy(p => p.Action)
                    .Select(p => new PermissionItemDto(
                        p.Id,
                        p.Action,
                        p.Description,
                        p.GetPermissionString(),
                        p.CreatedAt
                    ))]
            ))
            .ToList();

        // Return the grouped permissions wrapped in a success result
        return Result.Success(groupedPermissions);
    }
}
