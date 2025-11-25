using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Events;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// User aggregate root - Authentication
/// </summary>
public class User : AggregateRoot<Guid>
{
    public Email Email { get; private set; } = null!;
    public Password Password { get; private set; } = null!;
    public FullName FullName { get; private set; } = null!;
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsLocked { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    private readonly List<LoginHistory> _loginHistories = [];
    public IReadOnlyCollection<LoginHistory> LoginHistories => _loginHistories.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<UserAttribute> _userAttributes = [];
    public IReadOnlyCollection<UserAttribute> UserAttributes => _userAttributes.AsReadOnly();

    private readonly List<UserRelationship> _relationships = [];
    public IReadOnlyCollection<UserRelationship> Relationships => _relationships.AsReadOnly();

    private User() { } // EF Core

    private User(Email email, Password password, FullName fullName)
    {
        Id = Guid.NewGuid();
        Email = email;
        Password = password;
        FullName = fullName;
        IsEmailVerified = false;
        IsActive = true;
        IsLocked = false;
        FailedLoginAttempts = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<User> Create(Email email, Password password, FullName fullName)
    {
        var user = new User(email, password, fullName);
        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email));
        return Result.Success(user);
    }

    public Result UpdatePassword(Password newPassword)
    {
        Password = newPassword;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserPasswordChangedEvent(Id));
        return Result.Success();
    }

    public Result UpdateProfile(FullName fullName)
    {
        FullName = fullName;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result VerifyEmail()
    {
        if (IsEmailVerified)
            return Result.Failure("Email is already verified.");

        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserEmailVerifiedEvent(Id));
        return Result.Success();
    }

    public Result RecordSuccessfulLogin(string ipAddress, string userAgent)
    {
        if (!IsActive)
            return Result.Failure("User account is not active.");

        if (IsLocked && LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow)
            return Result.Failure($"Account is locked until {LockedUntil.Value}.");

        FailedLoginAttempts = 0;
        IsLocked = false;
        LockedUntil = null;
        LastLoginAt = DateTime.UtcNow;

        var loginHistory = LoginHistory.Create(Id, ipAddress, userAgent, true);
        _loginHistories.Add(loginHistory);

        AddDomainEvent(new UserLoggedInEvent(Id, ipAddress));
        return Result.Success();
    }

    public Result RecordFailedLogin(string ipAddress, string userAgent)
    {
        FailedLoginAttempts++;

        var loginHistory = LoginHistory.Create(Id, ipAddress, userAgent, false);
        _loginHistories.Add(loginHistory);

        if (FailedLoginAttempts >= 5)
        {
            IsLocked = true;
            LockedUntil = DateTime.UtcNow.AddMinutes(30);
            AddDomainEvent(new UserLockedEvent(Id, LockedUntil.Value));
            return Result.Failure("Account has been locked due to multiple failed login attempts.");
        }

        return Result.Failure("Invalid credentials.");
    }

    public RefreshToken CreateRefreshToken(string token, DateTime expiresAt)
    {
        var refreshToken = RefreshToken.Create(Id, token, expiresAt);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public Result RevokeRefreshToken(Guid tokenId)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.Id == tokenId);
        if (token == null)
            return Result.Failure("Refresh token not found.");

        token.Revoke();
        return Result.Success();
    }

    public void AddRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            return;

        var userRole = UserRole.Create(Id, roleId);
        _userRoles.Add(userRole);
        AddDomainEvent(new UserRoleAssignedEvent(Id, roleId));
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
            AddDomainEvent(new UserRoleRemovedEvent(Id, roleId));
        }
    }

    public void AddAttribute(string key, string value, string dataType = "string")
    {
        var existingAttr = _userAttributes.FirstOrDefault(a => a.AttributeKey == key);
        if (existingAttr != null)
        {
            existingAttr.UpdateValue(value);
        }
        else
        {
            var attribute = UserAttribute.Create(Id, key, value, dataType);
            _userAttributes.Add(attribute);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRelationship(Guid targetUserId, string relationshipType)
    {
        if (_relationships.Any(r => r.TargetUserId == targetUserId && r.RelationshipType == relationshipType))
            return;

        var relationship = UserRelationship.Create(Id, targetUserId, relationshipType);
        _relationships.Add(relationship);
    }

    public Result Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserDeactivatedEvent(Id));
        return Result.Success();
    }

    public Result Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
