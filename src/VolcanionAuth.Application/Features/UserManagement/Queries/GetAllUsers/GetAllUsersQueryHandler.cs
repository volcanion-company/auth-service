using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.UserManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.UserManagement.Queries.GetAllUsers;

/// <summary>
/// Handles queries to retrieve a paginated list of users, including their roles and permissions, based on specified
/// filtering and search criteria.
/// </summary>
/// <remarks>This handler supports filtering users by active status and searching by email or name. Pagination
/// parameters must be within valid ranges to ensure correct results. The returned user list includes detailed role and
/// permission information for each user.</remarks>
/// <param name="userRepository">The user data repository used to access and retrieve user information, including roles and permissions.</param>
public class GetAllUsersQueryHandler(IReadRepository<User> userRepository) : IRequestHandler<GetAllUsersQuery, Result<UserListDto>>
{
    /// <summary>
    /// Handles a request to retrieve a paginated list of users, applying optional filters for activity status and
    /// search terms.
    /// </summary>
    /// <remarks>The returned user list includes user roles and permissions. If no users match the filters,
    /// the list will be empty. The method enforces limits on pagination parameters to prevent invalid
    /// requests.</remarks>
    /// <param name="request">The query containing pagination parameters, filter options, and search criteria for retrieving users. The page
    /// number must be greater than 0, and the page size must be between 1 and 100.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A result containing a UserListDto with the filtered and paginated list of users. Returns a failure result if the
    /// page number or page size is outside the allowed range.</returns>
    public async Task<Result<UserListDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        // Validate page and page size
        if (request.Page < 1)
        {
            return Result.Failure<UserListDto>("Page number must be greater than 0");
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<UserListDto>("Page size must be between 1 and 100");
        }

        // Get all users with their roles
        var allUsers = await userRepository.GetAllUsersWithPermissionsAsync(cancellationToken);

        // Apply filters
        var filteredUsers = allUsers.AsQueryable();

        // Filter by active status
        if (!request.IncludeInactive)
        {
            filteredUsers = filteredUsers.Where(u => u.IsActive);
        }

        // Apply search term if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            filteredUsers = filteredUsers.Where(u =>
                u.Email.Value.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                u.FullName.FirstName.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                u.FullName.LastName.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase)
            );
        }

        // Get total count before pagination
        var totalCount = filteredUsers.Count();

        // Apply pagination and mapping
        var users = filteredUsers
            .OrderBy(u => u.Email.Value)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto(
                u.Id,
                u.Email.Value,
                u.FullName.FirstName,
                u.FullName.LastName,
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt,
                u.UserRoles.Select(ur => new UserRoleDto(
                    ur.RoleId,
                    ur.Role.Name,
                    ur.Role.RolePermissions.Select(rp => new UserPermissionDto(
                        rp.PermissionId,
                        rp.Permission.Resource,
                        rp.Permission.Action,
                        rp.Permission.GetPermissionString()
                    )).ToList()
                )).ToList(),
                u.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => new UserPermissionDto(
                        rp.PermissionId,
                        rp.Permission.Resource,
                        rp.Permission.Action,
                        rp.Permission.GetPermissionString()
                    ))
                    .DistinctBy(p => p.PermissionId)
                    .OrderBy(p => p.Resource)
                    .ThenBy(p => p.Action)
                    .ToList()
            ))
            .ToList();

        var result = new UserListDto(
            users,
            totalCount,
            request.Page,
            request.PageSize
        );

        return Result.Success(result);
    }
}
