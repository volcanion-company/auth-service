using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.CreateRole;

/// <summary>
/// Handler for creating a new role.
/// </summary>
public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IReadRepository<Role> _readRoleRepository;
    private readonly IReadRepository<Permission> _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
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

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role name already exists
        var allRoles = await _readRoleRepository.GetAllAsync(cancellationToken);
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
        if (request.PermissionIds != null && request.PermissionIds.Any())
        {
            var allPermissions = await _permissionRepository.GetAllAsync(cancellationToken);
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
        await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload role with permissions
        var createdRole = await _readRoleRepository.GetRoleWithPermissionsAsync(role.Id, cancellationToken);

        // Map to DTO
        var roleDto = new RoleDto(
            createdRole!.Id,
            createdRole.Name,
            createdRole.Description,
            createdRole.IsActive,
            createdRole.CreatedAt,
            createdRole.UpdatedAt,
            createdRole.RolePermissions.Select(rp => new RolePermissionDto(
                rp.PermissionId,
                rp.Permission.Resource,
                rp.Permission.Action,
                rp.Permission.GetPermissionString()
            )).ToList()
        );

        return Result.Success(roleDto);
    }
}
