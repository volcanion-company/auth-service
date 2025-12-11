using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.UserManagement.Common;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.UserManagement.Queries.GetAllUsers;

/// <summary>
/// Handler for retrieving a paginated list of users with filtering and search capabilities.
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<UserListDto>>
{
    private readonly IReadRepository<User> _userRepository;

    public GetAllUsersQueryHandler(IReadRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

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
        var allUsers = await _userRepository.GetAllUsersWithPermissionsAsync(cancellationToken);

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
                u.Email.Value.ToLower().Contains(searchLower) ||
                u.FullName.FirstName.ToLower().Contains(searchLower) ||
                u.FullName.LastName.ToLower().Contains(searchLower)
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
