using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Application.Features.PolicyManagement.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.PolicyManagement.Queries.GetAllPolicies;

/// <summary>
/// Handler for retrieving a paginated list of all policies.
/// </summary>
public class GetAllPoliciesQueryHandler : IRequestHandler<GetAllPoliciesQuery, Result<PaginatedPolicyResponse>>
{
    private readonly IReadRepository<Policy> _policyRepository;

    public GetAllPoliciesQueryHandler(IReadRepository<Policy> policyRepository)
    {
        _policyRepository = policyRepository;
    }

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
        var allPolicies = await _policyRepository.GetAllAsync(cancellationToken);

        // Filter by active status
        var filteredPolicies = request.IncludeInactive 
            ? allPolicies.ToList() 
            : allPolicies.Where(p => p.IsActive).ToList();

        // Filter by resource if provided
        if (!string.IsNullOrWhiteSpace(request.Resource))
        {
            filteredPolicies = filteredPolicies
                .Where(p => p.Resource.Equals(request.Resource, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Filter by search term if provided
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            filteredPolicies = filteredPolicies
                .Where(p => p.Name.ToLower().Contains(searchLower) || 
                           (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                           p.Resource.ToLower().Contains(searchLower) ||
                           p.Action.ToLower().Contains(searchLower))
                .ToList();
        }

        // Order by priority descending
        filteredPolicies = filteredPolicies.OrderByDescending(p => p.Priority).ToList();

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
