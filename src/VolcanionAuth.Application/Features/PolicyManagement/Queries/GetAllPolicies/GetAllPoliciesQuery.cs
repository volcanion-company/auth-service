using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Queries.GetAllPolicies;

/// <summary>
/// Query to retrieve a paginated list of all policies in the system.
/// </summary>
public record GetAllPoliciesQuery(
    int Page = 1,
    int PageSize = 10,
    bool IncludeInactive = false,
    string? Resource = null,
    string? SearchTerm = null
) : IRequest<Result<PaginatedPolicyResponse>>;

/// <summary>
/// Paginated response containing a list of policies.
/// </summary>
public record PaginatedPolicyResponse(
    List<PolicyDto> Policies,
    int CurrentPage,
    int PageSize,
    int TotalCount,
    int TotalPages
);
