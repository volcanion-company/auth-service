using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Defines methods for evaluating policies against a given context asynchronously.
/// </summary>
/// <remarks>Implementations of this interface are responsible for determining whether one or more policies are
/// satisfied based on the provided context data. Methods are asynchronous and support cancellation via a <see
/// cref="CancellationToken"/>. Thread safety and evaluation semantics may vary by implementation.</remarks>
public interface IPolicyEngineService
{
    /// <summary>
    /// Asynchronously evaluates the specified policy against the provided context data.
    /// </summary>
    /// <param name="policy">The policy to be evaluated. Cannot be null.</param>
    /// <param name="context">A dictionary containing contextual data used for policy evaluation. Keys represent context variable names;
    /// values provide their corresponding data. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the policy is
    /// satisfied; otherwise, <see langword="false"/>.</returns>
    Task<bool> EvaluatePolicyAsync(Policy policy, Dictionary<string, object> context, CancellationToken cancellationToken = default);
    /// <summary>
    /// Evaluates the specified policies asynchronously using the provided context data.
    /// </summary>
    /// <remarks>The evaluation is performed asynchronously and may involve multiple policies. The context
    /// dictionary should include all data required by the policies for accurate evaluation.</remarks>
    /// <param name="policies">The collection of policies to be evaluated. Cannot be null or contain null elements.</param>
    /// <param name="context">A dictionary containing contextual data used during policy evaluation. Keys represent context variable names;
    /// values provide their corresponding data. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a PolicyEvaluationResult describing
    /// the outcome of the policy evaluations.</returns>
    Task<PolicyEvaluationResult> EvaluatePoliciesAsync(IEnumerable<Policy> policies, Dictionary<string, object> context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of evaluating a policy, including whether access is allowed, the reason for the decision, and
/// the matched policy if applicable.
/// </summary>
/// <param name="IsAllowed">Indicates whether the action is permitted according to the evaluated policy. Set to <see langword="true"/> if access
/// is allowed; otherwise, <see langword="false"/>.</param>
/// <param name="Reason">A descriptive message explaining the reason for the policy evaluation result. This may include details about why
/// access was allowed or denied.</param>
/// <param name="MatchedPolicy">The policy that was matched during evaluation, if any; otherwise, <see langword="null"/> if no specific policy was
/// matched.</param>
public record PolicyEvaluationResult(
    bool IsAllowed,
    string Reason,
    Policy? MatchedPolicy = null
);
