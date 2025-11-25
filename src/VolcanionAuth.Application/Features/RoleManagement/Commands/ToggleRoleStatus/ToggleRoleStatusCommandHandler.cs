using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.ToggleRoleStatus;

/// <summary>
/// Handler for activating or deactivating a role.
/// </summary>
public class ToggleRoleStatusCommandHandler : IRequestHandler<ToggleRoleStatusCommand, Result<RoleDto>>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IReadRepository<Role> _readRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleRoleStatusCommandHandler(
        IRepository<Role> roleRepository,
        IReadRepository<Role> readRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _readRoleRepository = readRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleDto>> Handle(ToggleRoleStatusCommand request, CancellationToken cancellationToken)
    {
        // Find the role with permissions
        var role = await _readRoleRepository.GetRoleWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result.Failure<RoleDto>($"Role with ID '{request.RoleId}' was not found");
        }

        // Toggle status
        if (request.IsActive)
        {
            role.Activate();
        }
        else
        {
            role.Deactivate();
        }

        // Save changes
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var roleDto = new RoleDto(
            role.Id,
            role.Name,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt,
            role.RolePermissions.Select(rp => new RolePermissionDto(
                rp.PermissionId,
                rp.Permission.Resource,
                rp.Permission.Action,
                rp.Permission.GetPermissionString()
            )).ToList()
        );

        return Result.Success(roleDto);
    }
}
