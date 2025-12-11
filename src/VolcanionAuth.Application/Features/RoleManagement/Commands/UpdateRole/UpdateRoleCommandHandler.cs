using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.UpdateRole;

/// <summary>
/// Handler for updating an existing role's information.
/// </summary>
public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IReadRepository<Role> _readRoleRepository;
    private readonly IReadRepository<Permission> _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(
        IRepository<Role> roleRepository,
        IReadRepository<Role> readRoleRepository,
        IReadRepository<Permission> permissionRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _readRoleRepository = readRoleRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        // Find the role
        var role = await _readRoleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result.Failure<RoleDto>($"Role with ID '{request.RoleId}' was not found");
        }

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != role.Name)
        {
            // Check if new name already exists
            var allRoles = await _readRoleRepository.GetAllAsync(cancellationToken);
            if (allRoles.Any(r => r.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase) && r.Id != request.RoleId))
            {
                return Result.Failure<RoleDto>($"A role with the name '{request.Name}' already exists");
            }

            var updateResult = role.Update(request.Name, request.Description ?? role.Description);
            if (updateResult.IsFailure)
            {
                return Result.Failure<RoleDto>(updateResult.Error);
            }
        }
        else if (request.Description != null && request.Description != role.Description)
        {
            var updateResult = role.Update(role.Name, request.Description);
            if (updateResult.IsFailure)
            {
                return Result.Failure<RoleDto>(updateResult.Error);
            }
        }

        // Update permissions if provided
        if (request.PermissionIds != null)
        {
            var allPermissions = await _permissionRepository.GetAllAsync(cancellationToken);
            var requestedPermissions = allPermissions.Where(p => request.PermissionIds.Contains(p.Id)).ToList();

            if (requestedPermissions.Count != request.PermissionIds.Count)
            {
                var foundIds = requestedPermissions.Select(p => p.Id).ToList();
                var missingIds = request.PermissionIds.Except(foundIds).ToList();
                return Result.Failure<RoleDto>($"The following permission IDs were not found: {string.Join(", ", missingIds)}");
            }

            // Clear existing permissions
            var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();
            foreach (var permissionId in existingPermissionIds)
            {
                role.RemovePermission(permissionId);
            }

            // Add new permissions
            foreach (var permissionId in request.PermissionIds)
            {
                role.AddPermission(permissionId);
            }
        }

        // Save changes
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload role with permissions
        var updatedRole = await _readRoleRepository.GetRoleWithPermissionsAsync(request.RoleId, cancellationToken);

        // Map to DTO
        var roleDto = new RoleDto(
            updatedRole!.Id,
            updatedRole.Name,
            updatedRole.Description,
            updatedRole.IsActive,
            updatedRole.CreatedAt,
            updatedRole.UpdatedAt,
            updatedRole.RolePermissions.Select(rp => new RolePermissionDto(
                rp.PermissionId,
                rp.Permission.Resource,
                rp.Permission.Action,
                rp.Permission.GetPermissionString()
            )).ToList()
        );

        return Result.Success(roleDto);
    }
}
