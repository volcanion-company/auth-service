using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Policy engine for evaluating ABAC policies with conditions
/// </summary>
public interface IPolicyEngineService
{
    /// <summary>
    /// Evaluate a single policy against the provided context
    /// </summary>
    Task<bool> EvaluatePolicyAsync(Policy policy, Dictionary<string, object> context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluate multiple policies and return aggregated result based on priority and effect
    /// </summary>
    Task<PolicyEvaluationResult> EvaluatePoliciesAsync(
        IEnumerable<Policy> policies, 
        Dictionary<string, object> context, 
        CancellationToken cancellationToken = default);
}

public record PolicyEvaluationResult(
    bool IsAllowed,
    string Reason,
    Policy? MatchedPolicy = null
);
