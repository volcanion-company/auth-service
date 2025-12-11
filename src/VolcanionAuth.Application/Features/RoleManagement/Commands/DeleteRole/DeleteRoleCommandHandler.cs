using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.RoleManagement.Commands.DeleteRole;

/// <summary>
/// Handler for deleting a role from the system.
/// </summary>
public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IReadRepository<Role> _readRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(
        IRepository<Role> roleRepository,
        IReadRepository<Role> readRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _readRoleRepository = readRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        // Find the role
        var role = await _readRoleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result.Failure($"Role with ID '{request.RoleId}' was not found");
        }

        // Check if role has assigned users
        if (role.UserRoles.Any())
        {
            return Result.Failure($"Cannot delete role '{role.Name}' because it is assigned to {role.UserRoles.Count} user(s)");
        }

        // Delete the role
        _roleRepository.Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
