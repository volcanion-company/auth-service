namespace VolcanionAuth.Application.Features.UserManagement.Commands.DeleteUser;

/// <summary>
/// Represents a request to delete a user identified by a unique ID.
/// </summary>
/// <param name="UserId">The unique identifier of the user to be deleted. Must correspond to an existing user.</param>
public record DeleteUserCommand(Guid UserId) : IRequest<Result>;
