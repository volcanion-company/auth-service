using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.UserManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.AssignRoles;

/// <summary>
/// Handles the assign roles operation for a user by replacing all existing roles with the new set.
/// </summary>
/// <remarks>This handler validates the existence of the user and roles before assigning them. It clears
/// all existing roles and assigns the new ones atomically using the provided unit of work.</remarks>
/// <param name="userRepository">The repository used to persist changes to user entities.</param>
/// <param name="readUserRepository">The repository used to retrieve user entities for validation and data loading.</param>
/// <param name="roleRepository">The repository used to access role entities when assigning roles.</param>
/// <param name="userRoleRepository">The repository used to manage user-role associations.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store.</param>
public class AssignRolesCommandHandler(
    IRepository<User> userRepository,
    IReadRepository<User> readUserRepository,
    IReadRepository<Role> roleRepository,
    IRepository<UserRole> userRoleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignRolesCommand, Result<UserDto>>
{
    /// <summary>
    /// Handles the assigning of roles to a user by replacing all existing roles with the provided set.
    /// </summary>
    /// <param name="request">The command containing the user ID and the role IDs to assign.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A result containing the updated user data transfer object if the operation succeeds; otherwise, a failure
    /// result with an error message.</returns>
    public async Task<Result<UserDto>> Handle(AssignRolesCommand request, CancellationToken cancellationToken)
    {
        // Find the user using write repository to ensure it's tracked by the write context
        var user = await userRepository.GetUserWithRolesAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UserDto>($"User with ID '{request.UserId}' was not found");
        }

        // Validate that all role IDs exist
        var allRoles = await roleRepository.GetAllAsync(cancellationToken);
        var requestedRoles = allRoles.Where(r => request.RoleIds.Contains(r.Id)).ToList();

        if (requestedRoles.Count != request.RoleIds.Count)
        {
            var foundIds = requestedRoles.Select(r => r.Id).ToList();
            var missingIds = request.RoleIds.Except(foundIds).ToList();
            return Result.Failure<UserDto>($"The following role IDs were not found: {string.Join(", ", missingIds)}");
        }

        // Validate that all roles are active
        var inactiveRoles = requestedRoles.Where(r => !r.IsActive).ToList();
        if (inactiveRoles.Any())
        {
            var inactiveRoleNames = inactiveRoles.Select(r => r.Name);
            return Result.Failure<UserDto>($"The following roles are inactive and cannot be assigned: {string.Join(", ", inactiveRoleNames)}");
        }

        // Remove existing roles using repository to ensure proper tracking
        foreach (var userRole in user.UserRoles.ToList())
        {
            userRoleRepository.Remove(userRole);
        }

        // Add new roles directly as new entities
        foreach (var roleId in request.RoleIds)
        {
            var newUserRole = UserRole.Create(user.Id, roleId);
            await userRoleRepository.AddAsync(newUserRole, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload user with roles and permissions
        var updatedUser = await readUserRepository.GetUserWithPermissionsAsync(request.UserId, cancellationToken);

        // Get all user permissions
        var permissions = await readUserRepository.GetUserPermissionsAsync(request.UserId, cancellationToken);

        // Map to DTO
        var userDto = new UserDto(
            updatedUser!.Id,
            updatedUser.Email.Value,
            updatedUser.FullName.FirstName,
            updatedUser.FullName.LastName,
            updatedUser.IsActive,
            updatedUser.CreatedAt,
            updatedUser.LastLoginAt,
            [.. updatedUser.UserRoles.Select(ur => new UserRoleDto(
                ur.RoleId,
                ur.Role.Name,
                [.. ur.Role.RolePermissions.Select(rp => new UserPermissionDto(
                    rp.PermissionId,
                    rp.Permission.Resource,
                    rp.Permission.Action,
                    rp.Permission.GetPermissionString()
                ))]
            ))],
            [.. permissions.Select(p => new UserPermissionDto(
                p.Id,
                p.Resource,
                p.Action,
                p.GetPermissionString()
            ))]
        );

        return Result.Success(userDto);
    }
}
