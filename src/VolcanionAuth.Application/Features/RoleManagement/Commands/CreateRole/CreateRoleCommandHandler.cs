using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.CreateRole;

/// <summary>
/// Handles the creation of a new role, including validation of role name uniqueness and assignment of permissions.
/// </summary>
/// <remarks>This handler ensures that the role name is unique and that all specified permission IDs exist before
/// creating the role. If any permission IDs are invalid or the role name already exists, the operation fails with an
/// appropriate error message. The handler returns a data transfer object (DTO) representing the created role, including
/// its assigned permissions.</remarks>
/// <param name="roleRepository">The repository used to persist newly created roles.</param>
/// <param name="readRoleRepository">The repository used to query existing roles and retrieve role details with permissions.</param>
/// <param name="permissionRepository">The repository used to retrieve available permissions for assignment to roles.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store as part of the role creation process.</param>
public class CreateRoleCommandHandler(
    IRepository<Role> roleRepository,
    IReadRepository<Role> readRoleRepository,
    IReadRepository<Permission> permissionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    /// <summary>
    /// Handles the creation of a new role with the specified details and permissions.
    /// </summary>
    /// <remarks>If a role with the specified name already exists, the operation fails. All provided
    /// permission IDs must exist; otherwise, the operation fails and returns the missing IDs. The created role includes
    /// all assigned permissions.</remarks>
    /// <param name="request">The command containing the role name, description, and a list of permission IDs to assign to the new role.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the created role as a RoleDto if successful; otherwise, a failure result with an error
    /// message describing the reason for failure.</returns>
    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role name already exists
        var allRoles = await readRoleRepository.GetAllAsync(cancellationToken);
        if (allRoles.Any(r => r.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<RoleDto>($"A role with the name '{request.Name}' already exists");
        }

        // Create the role
        var roleResult = Role.Create(request.Name, request.Description);
        if (roleResult.IsFailure)
        {
            return Result.Failure<RoleDto>(roleResult.Error);
        }

        var role = roleResult.Value;

        // Assign permissions if provided
        if (request.PermissionIds != null && request.PermissionIds.Count != 0)
        {
            var allPermissions = await permissionRepository.GetAllAsync(cancellationToken);
            var requestedPermissions = allPermissions.Where(p => request.PermissionIds.Contains(p.Id)).ToList();

            if (requestedPermissions.Count != request.PermissionIds.Count)
            {
                var foundIds = requestedPermissions.Select(p => p.Id).ToList();
                var missingIds = request.PermissionIds.Except(foundIds).ToList();
                return Result.Failure<RoleDto>($"The following permission IDs were not found: {string.Join(", ", missingIds)}");
            }

            foreach (var permissionId in request.PermissionIds)
            {
                role.AddPermission(permissionId);
            }
        }

        // Save the role
        await roleRepository.AddAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload role with permissions
        var createdRole = await readRoleRepository.GetRoleWithPermissionsAsync(role.Id, cancellationToken);

        // Map to DTO
        var roleDto = new RoleDto(
            createdRole!.Id,
            createdRole.Name,
            createdRole.Description,
            createdRole.IsActive,
            createdRole.CreatedAt,
            createdRole.UpdatedAt,
            [.. createdRole.RolePermissions.Select(rp => new RolePermissionDto(
                rp.PermissionId,
                rp.Permission.Resource,
                rp.Permission.Action,
                rp.Permission.GetPermissionString()
            ))]
        );

        return Result.Success(roleDto);
    }
}
