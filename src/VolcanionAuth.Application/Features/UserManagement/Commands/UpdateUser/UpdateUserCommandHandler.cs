using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.UpdateUser;

/// <summary>
/// Handles update operations for user profiles in response to an UpdateUserCommand request.
/// </summary>
/// <remarks>This handler coordinates reading, updating, and saving user profile information. It ensures that user
/// updates are validated and persisted atomically. Thread safety and transactional integrity are managed via the
/// provided unit of work.</remarks>
/// <param name="userRepository">The repository used to update user entities in persistent storage.</param>
/// <param name="readUserRepository">The repository used to retrieve user entities for read operations.</param>
/// <param name="unitOfWork">The unit of work used to persist changes to the data store as part of the update operation.</param>
public class UpdateUserCommandHandler(
    IRepository<User> userRepository,
    IReadRepository<User> readUserRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, Result<UpdateUserResponse>>
{
    /// <summary>
    /// Handles the update of a user's profile information based on the specified update command.
    /// </summary>
    /// <remarks>Returns a failure result if the user is not found or if the provided profile information is
    /// invalid. The operation is performed asynchronously and persists changes to the underlying data store.</remarks>
    /// <param name="request">The command containing the user ID and updated profile information to apply. Must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the updated user information if the update succeeds; otherwise, a failure result with an
    /// error message.</returns>
    public async Task<Result<UpdateUserResponse>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Find the user
        var user = await readUserRepository.GetByIdAsync(request.UserId, cancellationToken);
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
        userRepository.Update(user);
        // Persist changes
        await unitOfWork.SaveChangesAsync(cancellationToken);
        // Return updated user info
        return Result.Success(new UpdateUserResponse(
            user.Id,
            user.Email.Value,
            user.FullName.FirstName,
            user.FullName.LastName
        ));
    }
}
