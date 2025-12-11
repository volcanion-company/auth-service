using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.UserManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.UserManagement.Queries.GetUserById;

/// <summary>
/// Handles queries to retrieve a user by their unique identifier, including associated roles and permissions.
/// </summary>
/// <remarks>This handler maps the retrieved user entity to a data transfer object (DTO) that includes user
/// details, roles, and permissions. If the specified user does not exist, the handler returns a failure result. The
/// handler is typically used in scenarios where complete user information, including authorization data, is required
/// for further processing.</remarks>
/// <param name="userRepository">The user data repository used to access user information and related permissions.</param>
public class GetUserByIdQueryHandler(IReadRepository<User> userRepository) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    /// <summary>
    /// Handles a query to retrieve a user by their unique identifier, including associated roles and permissions.
    /// </summary>
    /// <remarks>The returned user information includes all roles and permissions assigned to the user. If the
    /// specified user ID does not exist, the result will indicate failure and contain an appropriate error
    /// message.</remarks>
    /// <param name="request">The query containing the user ID to search for. Must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the user data transfer object if the user is found; otherwise, a failure result with an
    /// error message.</returns>
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
