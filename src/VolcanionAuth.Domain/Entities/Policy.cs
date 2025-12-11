using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Events;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents an access control policy that defines permissions for a specific resource and action, including effect,
/// conditions, and priority.
/// </summary>
/// <remarks>A policy specifies whether a particular action is allowed or denied on a resource, optionally under
/// certain conditions. Policies are typically used in authorization systems to determine access rights. The policy
/// includes metadata such as its name, description, activation status, and creation or update timestamps. Higher
/// priority values indicate that the policy should take precedence when multiple policies apply.</remarks>
public class Policy : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the name associated with this instance.
    /// </summary>
    public string Name { get; private set; } = null!;
    /// <summary>
    /// Gets the descriptive text associated with the object.
    /// </summary>
    public string? Description { get; private set; }
    /// <summary>
    /// Gets the identifier or path of the associated resource.
    /// </summary>
    public string Resource { get; private set; } = null!;
    /// <summary>
    /// Gets the name of the action associated with this instance.
    /// </summary>
    public string Action { get; private set; } = null!;
    /// <summary>
    /// Gets the effect that determines whether the associated action is allowed or denied.
    /// </summary>
    /// <remarks>The effect is typically set to either "Allow" or "Deny" to indicate the outcome of a policy
    /// or rule evaluation.</remarks>
    public string Effect { get; private set; } = null!;
    /// <summary>
    /// Gets the serialized conditions for the current object in JSON format.
    /// </summary>
    /// <remarks>The JSON string represents the set of conditions associated with this object. The structure
    /// and schema of the JSON are defined by the consuming application. This property is intended for scenarios where
    /// conditions need to be stored or transmitted in a standardized format.</remarks>
    public string Conditions { get; private set; } = null!;
    /// <summary>
    /// Gets a value indicating whether the object is currently active.
    /// </summary>
    public bool IsActive { get; private set; }
    /// <summary>
    /// Gets the priority level associated with the item. Higher values indicate higher priority.
    /// </summary>
    /// <remarks>The priority value is used to determine the order of processing or evaluation when multiple,
    /// higher-priority items take precedence over lower-priority ones. The specific range and meaning of priority
    /// </remarks>
    public int Priority { get; private set; }
    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// Gets the date and time when the entity was last updated, or null if the entity has not been updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Policy class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core when materializing objects from
    /// a database. It should not be called directly in application code.</remarks>
    private Policy() { }

    /// <summary>
    /// Initializes a new instance of the Policy class with the specified name, resource, action, effect, conditions,
    /// priority, and optional description.
    /// </summary>
    /// <param name="name">The unique name that identifies the policy. Cannot be null or empty.</param>
    /// <param name="resource">The resource to which the policy applies. Cannot be null or empty.</param>
    /// <param name="action">The action that the policy governs (for example, 'read', 'write', or 'delete'). Cannot be null or empty.</param>
    /// <param name="effect">The effect of the policy, such as 'allow' or 'deny'. Cannot be null or empty.</param>
    /// <param name="conditions">A string representing the conditions under which the policy is applicable. Cannot be null; may be an empty
    /// string if no conditions are required.</param>
    /// <param name="priority">The priority of the policy. Higher values indicate higher precedence when multiple policies apply.</param>
    /// <param name="description">An optional description providing additional information about the policy. Can be null.</param>
    private Policy(string name, string resource, string action, string effect, string conditions, int priority, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Resource = resource;
        Action = action;
        Effect = effect;
        Conditions = conditions;
        Priority = priority;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new policy definition with the specified parameters and returns the result of the creation operation.
    /// </summary>
    /// <param name="name">The unique name of the policy. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="resource">The resource to which the policy applies. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="action">The action that the policy governs. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="effect">The effect of the policy. Must be either "Allow" or "Deny".</param>
    /// <param name="conditions">The conditions under which the policy is enforced, represented as a string. May be null or empty if no
    /// conditions are required.</param>
    /// <param name="priority">The priority of the policy. Policies with higher priority values are evaluated before those with lower values.
    /// The default is 0.</param>
    /// <param name="description">An optional description of the policy. May be null.</param>
    /// <returns>A result containing the created policy if the parameters are valid; otherwise, a failure result with an error
    /// message describing the validation issue.</returns>
    public static Result<Policy> Create(string name, string resource, string action, string effect, string conditions, int priority = 0, string? description = null)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(name))
        {
            // Return a failure result if the name is null, empty, or consists only of white-space characters
            return Result.Failure<Policy>("Policy name cannot be empty.");
        }
        // Validate resource
        if (string.IsNullOrWhiteSpace(resource))
        {
            // Return a failure result if the resource is null, empty, or consists only of white-space characters
            return Result.Failure<Policy>("Resource cannot be empty.");
        }
        // Validate action
        if (string.IsNullOrWhiteSpace(action))
        {
            // Return a failure result if the action is null, empty, or consists only of white-space characters
            return Result.Failure<Policy>("Action cannot be empty.");
        }
        // Validate effect
        if (effect != "Allow" && effect != "Deny")
        {
            // Return a failure result if the effect is not "Allow" or "Deny"
            return Result.Failure<Policy>("Effect must be either 'Allow' or 'Deny'.");
        }
        // Validate conditions
        var policy = new Policy(name, resource, action, effect, conditions, priority, description);
        // Raise domain event for policy creation
        policy.AddDomainEvent(new PolicyCreatedEvent(policy.Id, policy.Name));
        // Return a success result with the created policy
        return Result.Success(policy);
    }

    /// <summary>
    /// Updates the rule's properties with the specified values.
    /// </summary>
    /// <param name="name">The new name to assign to the rule. Cannot be null.</param>
    /// <param name="resource">The resource to which the rule applies. Cannot be null.</param>
    /// <param name="action">The action that the rule governs. Cannot be null.</param>
    /// <param name="effect">The effect of the rule, such as 'allow' or 'deny'. Cannot be null.</param>
    /// <param name="conditions">The conditions under which the rule is applied. Cannot be null.</param>
    /// <param name="priority">The priority value to assign to the rule. Higher values indicate higher priority.</param>
    /// <param name="description">An optional description of the rule. May be null.</param>
    /// <returns>A Result object indicating whether the update operation was successful.</returns>
    public Result Update(string name, string resource, string action, string effect, string conditions, int priority, string? description)
    {
        Name = name;
        Resource = resource;
        Action = action;
        Effect = effect;
        Conditions = conditions;
        Priority = priority;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Deactivates the current instance and updates its status.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating whether the deactivation was successful.</returns>
    public Result Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Activates the current instance and updates its last modified timestamp.
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating whether the activation was successful.</returns>
    public Result Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
