using VolcanionAuth.Application.Features.PolicyManagement.Common;

namespace VolcanionAuth.Application.Features.PolicyManagement.Queries.GetPolicyById;

/// <summary>
/// Query to retrieve detailed information about a specific policy.
/// </summary>
public record GetPolicyByIdQuery(Guid PolicyId) : IRequest<Result<PolicyDto>>;
