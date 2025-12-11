using MediatR;
using VolcanionAuth.Application.Features.UserManagement.Common;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.UserManagement.Queries.GetUserById;

/// <summary>
/// Query to retrieve detailed information about a specific user by their ID.
/// </summary>
/// <param name="UserId">The unique identifier of the user to retrieve</param>
public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;
