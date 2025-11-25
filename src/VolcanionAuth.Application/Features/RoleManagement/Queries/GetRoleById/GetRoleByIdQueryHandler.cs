using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.RoleManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Queries.GetRoleById;

/// <summary>
/// Handler for retrieving detailed information about a specific role.
/// </summary>
public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    private readonly IReadRepository<Role> _roleRepository;

    public GetRoleByIdQueryHandler(IReadRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        // Find the role by ID with permissions
        var role = await _roleRepository.GetRoleWithPermissionsAsync(request.RoleId, cancellationToken);

        if (role == null)
        {
            return Result.Failure<RoleDto>($"Role with ID '{request.RoleId}' was not found");
        }

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
