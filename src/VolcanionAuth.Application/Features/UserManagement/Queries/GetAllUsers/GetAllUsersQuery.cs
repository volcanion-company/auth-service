using MediatR;
using VolcanionAuth.Application.Features.UserManagement.Common;
using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Application.Features.UserManagement.Queries.GetAllUsers;

/// <summary>
/// Query to retrieve a paginated list of all users in the system.
/// </summary>
/// <param name="Page">Page number (default: 1)</param>
/// <param name="PageSize">Number of items per page (default: 10)</param>
/// <param name="IncludeInactive">Whether to include inactive users (default: false)</param>
/// <param name="SearchTerm">Optional search term to filter users by name or email</param>
public record GetAllUsersQuery(
    int Page = 1,
    int PageSize = 10,
    bool IncludeInactive = false,
    string? SearchTerm = null
) : IRequest<Result<UserListDto>>;
