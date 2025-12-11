using MediatR;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.UserManagement.Commands.ToggleUserStatus;

/// <summary>
/// Command to activate or deactivate a user account.
/// </summary>
/// <param name="UserId">The ID of the user to toggle status</param>
/// <param name="IsActive">True to activate, false to deactivate</param>
public record ToggleUserStatusCommand(
    Guid UserId,
    bool IsActive
) : IRequest<Result<ToggleUserStatusResponse>>;

/// <summary>
/// Response object containing the updated user status.
/// </summary>
public record ToggleUserStatusResponse(
    Guid UserId,
    bool IsActive
);
