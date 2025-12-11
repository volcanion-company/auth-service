using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Queries.GetAllRoles;

/// <summary>
/// Handles queries to retrieve a paginated list of roles, including their associated permissions, based on filtering
/// and search criteria.
/// </summary>
/// <remarks>This handler supports filtering roles by active status and searching by name or description.
/// Pagination parameters must be within valid ranges; otherwise, the query will fail. The returned result includes both
/// the paginated role data and metadata about the total count and pages.</remarks>
/// <param name="roleRepository">The repository used to access role data, including roles and their permissions.</param>
public class GetAllRolesQueryHandler(IReadRepository<Role> roleRepository) : IRequestHandler<GetAllRolesQuery, Result<PaginatedRoleResponse>>
{
    /// <summary>
    /// Retrieves a paginated list of roles, including their associated permissions, based on the specified query
    /// parameters.
    /// </summary>
    /// <remarks>The method supports filtering roles by active status and by a search term applied to role
    /// names and descriptions. The page number must be greater than 0, and the page size must be between 1 and
    /// 100.</remarks>
    /// <param name="request">The query parameters that define pagination, filtering, and whether to include inactive roles.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing a paginated response of roles and their permissions. Returns a failure result if the
    /// pagination parameters are invalid.</returns>
    public async Task<Result<PaginatedRoleResponse>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        // Validate pagination parameters
        if (request.Page < 1)
        {
            return Result.Failure<PaginatedRoleResponse>("Page number must be greater than 0");
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<PaginatedRoleResponse>("Page size must be between 1 and 100");
        }

        // Get all roles with permissions
        var allRoles = await roleRepository.GetAllRolesWithPermissionsAsync(cancellationToken);

        // Filter by active status
        var filteredRoles = request.IncludeInactive ? allRoles : [.. allRoles.Where(r => r.IsActive)];

        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            filteredRoles = [.. filteredRoles
                .Where(r => r.Name.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) || 
                           (r.Description != null && r.Description.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase)))];
        }

        // Calculate pagination
        var totalCount = filteredRoles.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Apply pagination
        var paginatedRoles = filteredRoles
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to DTOs
        var roleDtos = paginatedRoles.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsActive,
            r.CreatedAt,
            r.UpdatedAt,
            [.. r.RolePermissions.Select(rp => new RolePermissionDto(
                rp.PermissionId,
                rp.Permission.Resource,
                rp.Permission.Action,
                rp.Permission.GetPermissionString()
            ))]
        )).ToList();

        var response = new PaginatedRoleResponse(
            roleDtos,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages
        );

        return Result.Success(response);
    }
}
