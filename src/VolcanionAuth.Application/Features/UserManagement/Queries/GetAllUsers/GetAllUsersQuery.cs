using VolcanionAuth.Application.Features.UserManagement.Common;

namespace VolcanionAuth.Application.Features.UserManagement.Queries.GetAllUsers;

/// <summary>
/// Represents a query to retrieve a paginated list of users, optionally including inactive users and filtering by a
/// search term.
/// </summary>
/// <param name="Page">The page number of results to retrieve. Must be greater than or equal to 1.</param>
/// <param name="PageSize">The maximum number of users to include in a single page of results. Must be greater than 0.</param>
/// <param name="IncludeInactive">Indicates whether inactive users should be included in the results. Set to <see langword="true"/> to include
/// inactive users; otherwise, only active users are returned.</param>
/// <param name="SearchTerm">An optional search term used to filter users by name or other identifying information. If <see langword="null"/>, no
/// filtering is applied.</param>
public record GetAllUsersQuery(
    int Page = 1,
    int PageSize = 10,
    bool IncludeInactive = false,
    string? SearchTerm = null
) : IRequest<Result<UserListDto>>;
