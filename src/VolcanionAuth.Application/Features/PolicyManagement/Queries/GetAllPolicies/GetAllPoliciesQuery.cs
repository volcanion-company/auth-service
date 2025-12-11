using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Queries.GetAllPolicies;

/// <summary>
/// Represents a query to retrieve a paginated list of policies, optionally filtered by resource, search term, and
/// active status.
/// </summary>
/// <remarks>This query supports pagination and flexible filtering to help clients efficiently browse and locate
/// policies. The returned result includes both the list of matching policies and pagination metadata.</remarks>
/// <param name="Page">The page number of results to retrieve. Must be greater than or equal to 1.</param>
/// <param name="PageSize">The maximum number of policies to include in a single page of results. Must be greater than 0.</param>
/// <param name="IncludeInactive">Indicates whether to include inactive policies in the results. Set to <see langword="true"/> to include inactive
/// policies; otherwise, only active policies are returned.</param>
/// <param name="Resource">An optional resource identifier used to filter policies by their associated resource. If <see langword="null"/>, no
/// resource filtering is applied.</param>
/// <param name="SearchTerm">An optional search term used to filter policies by name or description. If <see langword="null"/>, no search
/// filtering is applied.</param>
public record GetAllPoliciesQuery(
    int Page = 1,
    int PageSize = 10,
    bool IncludeInactive = false,
    string? Resource = null,
    string? SearchTerm = null
) : IRequest<Result<PaginatedPolicyResponse>>;

/// <summary>
/// Represents a paginated response containing a collection of policy records and pagination metadata.
/// </summary>
/// <remarks>Use this record to retrieve policy data in a paginated format, enabling efficient navigation through
/// large result sets. The pagination metadata can be used to implement paging controls in user interfaces.</remarks>
/// <param name="Policies">The list of policy records included in the current page of results. The list may be empty if no policies are
/// available for the specified page.</param>
/// <param name="CurrentPage">The zero-based index of the current page in the paginated result set.</param>
/// <param name="PageSize">The maximum number of policy records included in each page.</param>
/// <param name="TotalCount">The total number of policy records available across all pages.</param>
/// <param name="TotalPages">The total number of pages available based on the page size and total count.</param>
public record PaginatedPolicyResponse(
    List<PolicyDto> Policies,
    int CurrentPage,
    int PageSize,
    int TotalCount,
    int TotalPages
);
