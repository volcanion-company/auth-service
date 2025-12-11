using System.Text.Json;
using Microsoft.Extensions.Logging;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Infrastructure.Services;

/// <summary>
/// Policy engine implementation using JSON-based condition evaluation
/// Supports: equality, comparison, logical operators, time-based rules
/// </summary>
public class PolicyEngineService(ILogger<PolicyEngineService> logger) : IPolicyEngineService
{
    /// <summary>
    /// Asynchronously evaluates the specified policy against the provided context and returns a value indicating
    /// whether the policy conditions are satisfied.
    /// </summary>
    /// <remarks>If the policy does not define any conditions, the method returns <see langword="true"/>. If
    /// the policy conditions are invalid or an error occurs during evaluation, the method returns <see
    /// langword="false"/>. This method does not throw exceptions for invalid input or evaluation errors; instead, it
    /// logs the error and returns <see langword="false"/>.</remarks>
    /// <param name="policy">The policy to evaluate. Must not be null and must contain valid condition definitions.</param>
    /// <param name="context">A dictionary containing contextual data used to evaluate the policy conditions. Keys and values should match the
    /// expected inputs defined by the policy.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the policy
    /// conditions are satisfied; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> EvaluatePolicyAsync(Policy policy, Dictionary<string, object> context, CancellationToken cancellationToken = default)
    {
        try
        {
            // If there are no conditions, the policy always matches
            if (string.IsNullOrWhiteSpace(policy.Conditions) || policy.Conditions == "{}")
            {
                // No conditions = always true
                return true;
            }

            // Parse conditions from JSON
            var conditions = JsonSerializer.Deserialize<Dictionary<string, object>>(policy.Conditions);
            if (conditions == null)
            {
                // Log warning and return false for invalid conditions
                logger.LogWarning("Failed to parse policy conditions for policy {PolicyId}", policy.Id);
                return false;
            }
            // Evaluate conditions against context
            var result = EvaluateConditions(conditions, context);
            // Log evaluation result
            logger.LogDebug("Policy {PolicyName} evaluated to {Result} with context {Context}", policy.Name, result, JsonSerializer.Serialize(context));
            // Return evaluation result
            return result;
        }
        catch (Exception ex)
        {
            // Log exception and return false
            logger.LogError(ex, "Error evaluating policy {PolicyId}", policy.Id);
            // Return false on error
            return false;
        }
    }

    /// <summary>
    /// Evaluates the specified policies against the provided context and determines whether access is allowed based on
    /// the highest-priority matching policy.
    /// </summary>
    /// <remarks>Policies are evaluated in order of descending priority. The first active policy that matches
    /// the context determines the result. If no active or matching policy is found, access is denied by
    /// default.</remarks>
    /// <param name="policies">A collection of policies to evaluate. Only active policies are considered during evaluation.</param>
    /// <param name="context">A dictionary containing contextual information used for policy evaluation. Keys represent context variable
    /// names, and values provide their corresponding data.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a PolicyEvaluationResult indicating
    /// whether access is allowed, the reason for the decision, and the matched policy if applicable.</returns>
    public async Task<PolicyEvaluationResult> EvaluatePoliciesAsync(
        IEnumerable<Policy> policies,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default)
    {
        // Order policies by priority (highest first) and filter active ones
        var orderedPolicies = policies
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Priority)
            .ToList();
        // If no active policies, deny by default
        if (orderedPolicies.Count == 0)
        {
            // return denial
            return new PolicyEvaluationResult(false, "No active policies found");
        }

        // Evaluate each policy in order
        foreach (var policy in orderedPolicies)
        {
            // Evaluate policy
            var matches = await EvaluatePolicyAsync(policy, context, cancellationToken);
            if (matches)
            {
                // Set result based on policy effect
                var isAllowed = policy.Effect == "Allow";
                // Create reason message
                var reason = $"Matched policy '{policy.Name}' with effect '{policy.Effect}'";
                // Log match
                logger.LogInformation("Policy match: {PolicyName} (Priority: {Priority}, Effect: {Effect})", policy.Name, policy.Priority, policy.Effect);
                // Return result
                return new PolicyEvaluationResult(isAllowed, reason, policy);
            }
        }

        // No matching policies found, deny by default
        return new PolicyEvaluationResult(false, "No matching policies");
    }

    /// <summary>
    /// Evaluates whether the specified set of conditions are satisfied by the provided context values.
    /// </summary>
    /// <remarks>Supports logical operators such as "$and" and "$or", as well as comparison and time-based
    /// conditions. Condition keys and values should follow the expected structure for correct evaluation.</remarks>
    /// <param name="conditions">A dictionary containing condition keys and their expected values. Keys may represent logical operators (such as
    /// "$and" or "$or"), comparison operations, or direct context keys.</param>
    /// <param name="context">A dictionary representing the current context, where each key corresponds to a value that may be evaluated
    /// against the conditions.</param>
    /// <returns>true if all conditions are satisfied by the context; otherwise, false.</returns>
    private static bool EvaluateConditions(Dictionary<string, object> conditions, Dictionary<string, object> context)
    {
        // Evaluate each condition
        foreach (var condition in conditions)
        {
            // Extract key and expected value
            var key = condition.Key;
            var expectedValue = condition.Value;

            // Handle logical operators
            if (key.Equals("$and", StringComparison.OrdinalIgnoreCase))
            {
                // All sub-conditions must be true
                var subConditions = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(expectedValue.ToString()!);
                if (subConditions == null || !subConditions.All(c => EvaluateConditions(c, context)))
                {
                    // Return false if any sub-condition fails
                    return false;
                }
                continue;
            }

            // Handle logical OR
            if (key.Equals("$or", StringComparison.OrdinalIgnoreCase))
            {
                // At least one sub-condition must be true
                var subConditions = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(expectedValue.ToString()!);
                if (subConditions == null || !subConditions.Any(c => EvaluateConditions(c, context)))
                {
                    // Return false if none are true
                    return false;
                }
                continue;
            }

            // Handle special time-based conditions
            if (key.Equals("$timeRange", StringComparison.OrdinalIgnoreCase))
            {
                // Evaluate time range condition
                if (!EvaluateTimeRange(expectedValue, context))
                {
                    // Return false if time condition fails
                    return false;
                }
                continue;
            }

            // Handle comparison operators
            if (key.Contains('.'))
            {
                // Split key into context key and operation
                var parts = key.Split('.');
                var contextKey = parts[0];
                var operation = parts[1];
                // Get actual value from context
                if (!context.TryGetValue(contextKey, out var actualValue))
                {
                    // Context key not found
                    return false;
                }
                // Evaluate comparison
                if (!EvaluateComparison(actualValue, expectedValue, operation))
                {
                    // Return false if comparison fails
                    return false;
                }

                continue;
            }

            // Simple equality check
            if (!context.TryGetValue(key, out var value))
            {
                // Context key not found
                return false;
            }
            // Check for equality
            if (!AreValuesEqual(value, expectedValue))
            {
                // Return false if values are not equal
                return false;
            }
        }

        // All conditions passed
        return true;
    }

    /// <summary>
    /// Evaluates a comparison between two values using the specified comparison operation.
    /// </summary>
    /// <remarks>If the operation requires numeric comparison ("gt", "gte", "lt", "lte"), both values must be
    /// convertible to double. For the "in" operation, the expected value should be a collection containing the actual
    /// value. For the "contains" operation, both values are converted to strings and compared using a case-insensitive
    /// search.</remarks>
    /// <param name="actual">The value to be compared. Can be any object that is compatible with the specified operation.</param>
    /// <param name="expected">The value to compare against. The expected value's type and meaning depend on the operation.</param>
    /// <param name="operation">The comparison operation to perform. Supported values are: "eq" (equal), "ne" (not equal), "gt" (greater than),
    /// "gte" (greater than or equal), "lt" (less than), "lte" (less than or equal), "in" (contained in list), and
    /// "contains" (string contains). The comparison is case-insensitive.</param>
    /// <returns>true if the comparison succeeds according to the specified operation; otherwise, false. Returns false if the
    /// operation is not recognized or if the comparison cannot be performed due to incompatible types.</returns>
    private static bool EvaluateComparison(object actual, object expected, string operation)
    {
        try
        {
            // Perform comparison based on operation
            return operation.ToLower() switch
            {
                "eq" => AreValuesEqual(actual, expected),
                "ne" => !AreValuesEqual(actual, expected),
                "gt" => Convert.ToDouble(actual) > Convert.ToDouble(expected),
                "gte" => Convert.ToDouble(actual) >= Convert.ToDouble(expected),
                "lt" => Convert.ToDouble(actual) < Convert.ToDouble(expected),
                "lte" => Convert.ToDouble(actual) <= Convert.ToDouble(expected),
                "in" => IsValueInList(actual, expected),
                "contains" => actual.ToString()?.Contains(expected.ToString() ?? "", StringComparison.OrdinalIgnoreCase) ?? false,
                _ => false
            };
        }
        catch
        {
            // Return false on any error during comparison
            return false;
        }
    }

    /// <summary>
    /// Determines whether the current time falls within the specified time range condition.
    /// </summary>
    /// <remarks>If the time range condition cannot be parsed or required context is missing or invalid, the
    /// method returns false.</remarks>
    /// <param name="expectedValue">An object representing the expected time range condition, serialized as a JSON string. The object must contain
    /// 'Start' and 'End' properties in a format parseable as TimeSpan.</param>
    /// <param name="context">A dictionary containing contextual information. The value associated with the 'currentTime' key, if present, is
    /// used as the current time; otherwise, the current UTC time is used.</param>
    /// <returns>true if the current time is within the specified time range; otherwise, false.</returns>
    private static bool EvaluateTimeRange(object expectedValue, Dictionary<string, object> context)
    {
        try
        {
            // Deserialize time range condition
            var timeRange = JsonSerializer.Deserialize<TimeRangeCondition>(expectedValue.ToString()!);
            if (timeRange == null)
            {
                // Return false for invalid time range
                return false;
            }

            // Get current time from context or use UTC now
            var currentTime = context.TryGetValue("currentTime", out var contextTime) ? DateTime.Parse(contextTime.ToString()!) : DateTime.UtcNow;
            // Parse start and end times
            var start = TimeSpan.Parse(timeRange.Start);
            var end = TimeSpan.Parse(timeRange.End);
            // Get current time of day
            var current = currentTime.TimeOfDay;
            // Check if current time is within range
            return current >= start && current <= end;
        }
        catch
        {
            // Return false on any error
            return false;
        }
    }

    /// <summary>
    /// Determines whether two values are equal, performing a case-insensitive comparison and handling JSON elements
    /// appropriately.
    /// </summary>
    /// <remarks>If either parameter is a System.Text.Json.JsonElement, its underlying value is extracted
    /// before comparison. The comparison is case-insensitive and treats two null values as equal.</remarks>
    /// <param name="actual">The first value to compare. Can be a primitive type, string, or a JSON element.</param>
    /// <param name="expected">The second value to compare. Can be a primitive type, string, or a JSON element.</param>
    /// <returns>true if the values are considered equal; otherwise, false.</returns>
    private static bool AreValuesEqual(object actual, object expected)
    {
        // Handle null values
        if (actual == null && expected == null)
        {
            // Both null = equal
            return true;
        }
        // If one is null, not equal
        if (actual == null || expected == null)
        {
            // One null = not equal
            return false;
        }

        // Handle actual JsonElement
        var actualValue = actual is JsonElement jsonElement ? global::VolcanionAuth.Infrastructure.Services.PolicyEngineService.GetJsonElementValue(jsonElement) : actual;
        // Handle expected JsonElement
        var expectedValue = expected is JsonElement jsonElement2 ? global::VolcanionAuth.Infrastructure.Services.PolicyEngineService.GetJsonElementValue(jsonElement2) : expected;
        // Perform case-insensitive comparison
        return actualValue?.ToString()?.Equals(expectedValue?.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Determines whether the specified value is present in a list represented by the expected object.
    /// </summary>
    /// <remarks>If the expected parameter cannot be deserialized as a JSON array, the method returns false.
    /// Value comparison is performed using a custom equality check.</remarks>
    /// <param name="actual">The value to search for within the list.</param>
    /// <param name="expected">An object whose string representation is expected to be a JSON array containing the values to search.</param>
    /// <returns>true if the actual value is found in the list; otherwise, false.</returns>
    private static bool IsValueInList(object actual, object expected)
    {
        try
        {
            // Deserialize expected value as list
            var list = JsonSerializer.Deserialize<List<object>>(expected.ToString()!);
            // Check if actual value is in the list
            return list?.Any(item => AreValuesEqual(actual, item)) ?? false;
        }
        catch
        {
            // Return false on any error
            return false;
        }
    }

    /// <summary>
    /// Extracts the underlying value from a specified <see cref="JsonElement"/> as a .NET object.
    /// </summary>
    /// <remarks>For JSON objects and arrays, the method returns the string representation of the element
    /// rather than a structured object or collection.</remarks>
    /// <param name="element">The <see cref="JsonElement"/> from which to retrieve the value.</param>
    /// <returns>A .NET object representing the value of the JSON element. Returns a <see langword="string"/> for JSON strings, a
    /// <see langword="double"/> for numbers, <see langword="true"/> or <see langword="false"/> for boolean values, <see
    /// langword="null"/> for JSON null, or a string representation for other value kinds.</returns>
    private static object? GetJsonElementValue(JsonElement element)
    {
        // Extract value based on JSON value kind
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    /// <summary>
    /// Represents a condition defined by a start and end time range.
    /// </summary>
    private class TimeRangeCondition
    {
        /// <summary>
        /// Gets or sets the start value for the range or interval represented by this instance.
        /// </summary>
        public string Start { get; set; } = null!;

        /// <summary>
        /// Gets or sets the end value associated with this instance.
        /// </summary>
        public string End { get; set; } = null!;
    }
}
