using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.UserManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.UserManagement.Queries.GetUserById;

/// <summary>
/// Handler for retrieving detailed information about a specific user.
/// </summary>
public class GetUserByIdQueryHandler(IReadRepository<User> userRepository) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // Find the user by ID with permissions
        var user = await userRepository.GetUserWithPermissionsAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UserDto>($"User with ID '{request.UserId}' was not found");
        }

        // Map to DTO
        var userDto = new UserDto(
            user.Id,
            user.Email.Value,
            user.FullName.FirstName,
            user.FullName.LastName,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            [.. user.UserRoles.Select(ur => new UserRoleDto(
                ur.RoleId,
                ur.Role.Name,
                [.. ur.Role.RolePermissions.Select(rp => new UserPermissionDto(
                    rp.PermissionId,
                    rp.Permission.Resource,
                    rp.Permission.Action,
                    rp.Permission.GetPermissionString()
                ))]
            ))],
            [.. user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => new UserPermissionDto(
                    rp.PermissionId,
                    rp.Permission.Resource,
                    rp.Permission.Action,
                    rp.Permission.GetPermissionString()
                ))
                .DistinctBy(p => p.PermissionId)
                .OrderBy(p => p.Resource)
                .ThenBy(p => p.Action)]
        );

        return Result.Success(userDto);
    }
}
