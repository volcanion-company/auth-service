using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.DeleteUser;

/// <summary>
/// Command to permanently delete a user from the system.
/// </summary>
/// <param name="UserId">The ID of the user to delete</param>
public record DeleteUserCommand(Guid UserId) : IRequest<Result>;
