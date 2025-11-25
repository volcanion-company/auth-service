using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.UpdateUser;

/// <summary>
/// Handler for updating an existing user's information.
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UpdateUserResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IReadRepository<User> _readUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        IRepository<User> userRepository,
        IReadRepository<User> readUserRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _readUserRepository = readUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateUserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Find the user
        var user = await _readUserRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<UpdateUserResponse>($"User with ID '{request.UserId}' was not found");
        }

        // Update FullName if FirstName or LastName is provided
        if (!string.IsNullOrWhiteSpace(request.FirstName) || !string.IsNullOrWhiteSpace(request.LastName))
        {
            var firstName = request.FirstName ?? user.FullName.FirstName;
            var lastName = request.LastName ?? user.FullName.LastName;

            var fullNameResult = FullName.Create(firstName, lastName);
            if (fullNameResult.IsFailure)
            {
                return Result.Failure<UpdateUserResponse>(fullNameResult.Error);
            }

            var updateResult = user.UpdateProfile(fullNameResult.Value);
            if (updateResult.IsFailure)
            {
                return Result.Failure<UpdateUserResponse>(updateResult.Error);
            }
        }

        // Save changes
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateUserResponse(
            user.Id,
            user.Email.Value,
            user.FullName.FirstName,
            user.FullName.LastName
        ));
    }
}
