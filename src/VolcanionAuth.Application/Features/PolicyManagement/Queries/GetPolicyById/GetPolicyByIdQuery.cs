using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Queries.GetPolicyById;

/// <summary>
/// Represents a query to retrieve a policy by its unique identifier.
/// </summary>
/// <param name="PolicyId">The unique identifier of the policy to retrieve.</param>
public record GetPolicyByIdQuery(Guid PolicyId) : IRequest<Result<PolicyDto>>;
