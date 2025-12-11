using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Queries.GetAllPolicies;

/// <summary>
/// Handles queries to retrieve a paginated list of policies, supporting filtering by active status, resource, and
/// search term.
/// </summary>
/// <remarks>This handler validates pagination parameters and applies filtering and sorting based on the query
/// request. The result includes pagination metadata and a list of policy DTOs. The maximum allowed page size is 100.
/// This handler is typically used in scenarios where clients need to browse or search policies with flexible filtering
/// options.</remarks>
/// <param name="policyRepository">The repository used to access and retrieve policy entities from the data store.</param>
public class GetAllPoliciesQueryHandler(IReadRepository<Policy> policyRepository) : IRequestHandler<GetAllPoliciesQuery, Result<PaginatedPolicyResponse>>
{
    /// <summary>
    /// Handles a request to retrieve a paginated list of policies, applying optional filters for activity status,
    /// resource, and search terms.
    /// </summary>
    /// <remarks>Policies are ordered by descending priority before pagination is applied. Filtering is
    /// performed based on the specified resource, search term, and activity status. The returned response includes
    /// pagination metadata such as total count and total pages.</remarks>
    /// <param name="request">The query containing pagination parameters and optional filters for policies, such as resource, search term, and
    /// whether to include inactive policies. The page number must be greater than 0, and the page size must be between
    /// 1 and 100.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing a paginated response with the filtered list of policies. Returns a failure result if
    /// pagination parameters are invalid.</returns>
    public async Task<Result<PaginatedPolicyResponse>> Handle(GetAllPoliciesQuery request, CancellationToken cancellationToken)
    {
        // Validate pagination parameters
        if (request.Page < 1)
        {
            return Result.Failure<PaginatedPolicyResponse>("Page number must be greater than 0");
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<PaginatedPolicyResponse>("Page size must be between 1 and 100");
        }

        // Get all policies
        var allPolicies = await policyRepository.GetAllAsync(cancellationToken);

        // Filter by active status
        var filteredPolicies = request.IncludeInactive ? [.. allPolicies] : allPolicies.Where(p => p.IsActive).ToList();

        // Filter by resource if provided
        if (!string.IsNullOrWhiteSpace(request.Resource))
        {
            filteredPolicies = [.. filteredPolicies.Where(p => p.Resource.Equals(request.Resource, StringComparison.OrdinalIgnoreCase))];
        }

        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            filteredPolicies = [.. filteredPolicies
                .Where(p => p.Name.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                           (p.Description != null && p.Description.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase)) ||
                           p.Resource.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                           p.Action.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase))];
        }

        // Order by priority descending
        filteredPolicies = [.. filteredPolicies.OrderByDescending(p => p.Priority)];

        // Calculate pagination
        var totalCount = filteredPolicies.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Apply pagination
        var paginatedPolicies = filteredPolicies
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Map to DTOs
        var policyDtos = paginatedPolicies.Select(p => new PolicyDto(
            p.Id,
            p.Name,
            p.Description,
            p.Resource,
            p.Action,
            p.Effect,
            p.Conditions,
            p.Priority,
            p.IsActive,
            p.CreatedAt,
            p.UpdatedAt
        )).ToList();

        var response = new PaginatedPolicyResponse(
            policyDtos,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages
        );

        return Result.Success(response);
    }
}
