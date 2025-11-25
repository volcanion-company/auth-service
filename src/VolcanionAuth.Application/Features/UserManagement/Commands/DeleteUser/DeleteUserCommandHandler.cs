using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.DeleteUser;

/// <summary>
/// Handler for permanently deleting a user from the system.
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IRepository<User> _userRepository;
    private readonly IReadRepository<User> _readUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        IRepository<User> userRepository,
        IReadRepository<User> readUserRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _readUserRepository = readUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Find the user
        var user = await _readUserRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure($"User with ID '{request.UserId}' was not found");
        }

        // Delete the user
        _userRepository.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
