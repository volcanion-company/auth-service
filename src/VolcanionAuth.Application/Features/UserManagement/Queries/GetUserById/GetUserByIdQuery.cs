using VolcanionAuth.Application.Features.UserManagement.Common;

namespace VolcanionAuth.Application.Features.UserManagement.Queries.GetUserById;

/// <summary>
/// Represents a query to retrieve a user by their unique identifier.
/// </summary>
/// <param name="UserId">The unique identifier of the user to retrieve.</param>
public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;
