namespace VolcanionAuth.Application.Features.UserManagement.Commands.ToggleUserStatus;

/// <summary>
/// Represents a request to update a user's active status.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose status is to be toggled. Must be a valid, non-empty <see
/// cref="System.Guid"/>.</param>
/// <param name="IsActive">A value indicating whether the user should be marked as active. Set to <see langword="true"/> to activate the user;
/// otherwise, <see langword="false"/>.</param>
public record ToggleUserStatusCommand(
    Guid UserId,
    bool IsActive
) : IRequest<Result<ToggleUserStatusResponse>>;

/// <summary>
/// Represents the result of a user status toggle operation, including the user's identifier and their updated active
/// state.
/// </summary>
/// <param name="UserId">The unique identifier of the user whose status was toggled.</param>
/// <param name="IsActive">Indicates whether the user is now active. Set to <see langword="true"/> if the user is active; otherwise, <see
/// langword="false"/>.</param>
public record ToggleUserStatusResponse(
    Guid UserId,
    bool IsActive
);
