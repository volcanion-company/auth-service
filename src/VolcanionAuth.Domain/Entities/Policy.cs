using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Events;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Policy entity for ABAC (Attribute-Based Access Control)
/// </summary>
public class Policy : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Resource { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string Effect { get; private set; } = null!; // Allow / Deny
    public string Conditions { get; private set; } = null!; // JSON format
    public bool IsActive { get; private set; }
    public int Priority { get; private set; } // Higher number = higher priority
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Policy() { } // EF Core

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

    public static Result<Policy> Create(string name, string resource, string action, string effect, string conditions, int priority = 0, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Policy>("Policy name cannot be empty.");

        if (string.IsNullOrWhiteSpace(resource))
            return Result.Failure<Policy>("Resource cannot be empty.");

        if (string.IsNullOrWhiteSpace(action))
            return Result.Failure<Policy>("Action cannot be empty.");

        if (effect != "Allow" && effect != "Deny")
            return Result.Failure<Policy>("Effect must be either 'Allow' or 'Deny'.");

        var policy = new Policy(name, resource, action, effect, conditions, priority, description);
        policy.AddDomainEvent(new PolicyCreatedEvent(policy.Id, policy.Name));
        return Result.Success(policy);
    }

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

    public Result Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
