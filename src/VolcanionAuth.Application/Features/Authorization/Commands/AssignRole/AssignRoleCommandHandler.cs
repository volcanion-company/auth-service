using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.Authorization.Commands.AssignRole;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public AssignRoleCommandHandler(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result.Failure("User not found.");

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
            return Result.Failure("Role not found.");

        user.AddRole(request.RoleId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate user cache
        await _cacheService.RemoveAsync($"user_session:{request.UserId}", cancellationToken);
        await _cacheService.RemoveAsync($"user_permissions:{request.UserId}", cancellationToken);

        return Result.Success();
    }
}
