using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.ToggleUserStatus;

/// <summary>
/// Handler for toggling a user's active status.
/// </summary>
public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, Result<ToggleUserStatusResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IReadRepository<User> _readUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleUserStatusCommandHandler(
        IRepository<User> userRepository,
        IReadRepository<User> readUserRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _readUserRepository = readUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ToggleUserStatusResponse>> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        // Find the user
        var user = await _readUserRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<ToggleUserStatusResponse>($"User with ID '{request.UserId}' was not found");
        }

        // Toggle the status
        if (request.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        // Save changes
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ToggleUserStatusResponse(
            user.Id,
            user.IsActive
        ));
    }
}
