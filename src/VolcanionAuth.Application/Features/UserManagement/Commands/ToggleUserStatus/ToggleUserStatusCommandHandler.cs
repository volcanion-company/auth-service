using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.ToggleUserStatus;

/// <summary>
/// Handles the command to toggle a user's active status by activating or deactivating the specified user.
/// </summary>
/// <remarks>This handler updates the user's status and persists the change atomically. It returns a result
/// indicating success or failure, including details about the user's new status. The operation is asynchronous and
/// supports cancellation via the provided token.</remarks>
/// <param name="userRepository">The repository used to update user entities in the data store.</param>
/// <param name="readUserRepository">The repository used to retrieve user entities for read operations.</param>
/// <param name="unitOfWork">The unit of work used to persist changes to the data store.</param>
public class ToggleUserStatusCommandHandler(
    IRepository<User> userRepository,
    IReadRepository<User> readUserRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleUserStatusCommand, Result<ToggleUserStatusResponse>>
{
    /// <summary>
    /// Handles a request to toggle a user's active status and persists the change.
    /// </summary>
    /// <remarks>This method updates the user's status and saves the changes to the data store. The operation
    /// is performed asynchronously.</remarks>
    /// <param name="request">The command containing the user ID and the desired active status to apply.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing a response with the user's ID and updated active status. Returns a failure result if the
    /// user is not found.</returns>
    public async Task<Result<ToggleUserStatusResponse>> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        // Find the user
        var user = await readUserRepository.GetByIdAsync(request.UserId, cancellationToken);
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
        userRepository.Update(user);
        // Persist changes atomically
        await unitOfWork.SaveChangesAsync(cancellationToken);
        // Return response
        return Result.Success(new ToggleUserStatusResponse(
            user.Id,
            user.IsActive
        ));
    }
}
