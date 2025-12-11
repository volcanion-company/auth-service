using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Queries.GetAllRoles;

/// <summary>
/// Handler for retrieving a paginated list of all roles.
/// </summary>
public class GetAllRolesQueryHandler(IReadRepository<Role> roleRepository) : IRequestHandler<GetAllRolesQuery, Result<PaginatedRoleResponse>>
{
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
