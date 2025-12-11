using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.DeleteUser;

/// <summary>
/// Handles the command to delete a user by removing the specified user from the repository and persisting the change.
/// </summary>
/// <remarks>This handler ensures that the user exists before attempting deletion. If the user is not found, the
/// operation fails and no changes are made. The deletion is committed atomically using the provided unit of
/// work.</remarks>
/// <param name="userRepository">The repository used to remove user entities from persistent storage.</param>
/// <param name="unitOfWork">The unit of work used to commit changes to the data store after the user is deleted.</param>
public class DeleteUserCommandHandler(
    IRepository<User> userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteUserCommand, Result>
{
    /// <summary>
    /// Handles the deletion of a user specified by the command request.
    /// </summary>
    /// <param name="request">The command containing the user ID of the user to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the delete operation.</param>
    /// <returns>A result indicating whether the user was successfully deleted. Returns a failure result if the user does not
    /// exist.</returns>
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Find the user from WRITE repository
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure($"User with ID '{request.UserId}' was not found");
        }

        // Delete the user
        userRepository.Remove(user);
        // Persist the changes
        await unitOfWork.SaveChangesAsync(cancellationToken);
        // Return success
        return Result.Success();
    }
}
