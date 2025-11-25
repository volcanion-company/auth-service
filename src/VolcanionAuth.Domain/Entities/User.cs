using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Events;
using VolcanionAuth.Domain.ValueObjects;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents an application user, including authentication credentials, profile information, account status, and
/// related entities such as roles, attributes, relationships, login history, and refresh tokens.
/// </summary>
/// <remarks>The User class encapsulates user identity, authentication state, and related domain events. It
/// provides methods for managing user credentials, verifying email, tracking login attempts, handling account lockout,
/// managing roles and attributes, and maintaining relationships with other users. This class is typically used as an
/// aggregate root in domain-driven design and is intended to be persisted using an ORM such as Entity Framework
/// Core.</remarks>
public class User : AggregateRoot<Guid>
{
    /// <summary>
    /// Gets the email address associated with this instance.
    /// </summary>
    public Email Email { get; private set; } = null!;
    /// <summary>
    /// Gets the password associated with the current instance.
    /// </summary>
    public Password Password { get; private set; } = null!;
    /// <summary>
    /// Gets the full name associated with the entity.
    /// </summary>
    public FullName FullName { get; private set; } = null!;
    /// <summary>
    /// Gets a value indicating whether the user's email address has been verified.
    /// </summary>
    public bool IsEmailVerified { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the object is currently active.
    /// </summary>
    public bool IsActive { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the object is currently locked.
    /// </summary>
    public bool IsLocked { get; private set; }
    /// <summary>
    /// Gets the number of consecutive failed login attempts for the user.
    /// </summary>
    public int FailedLoginAttempts { get; private set; }
    /// <summary>
    /// Gets the date and time when the user last logged in, or null if the user has never logged in.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }
    /// <summary>
    /// Gets the date and time until which the resource remains locked, or null if the resource is not currently locked.
    /// </summary>
    public DateTime? LockedUntil { get; private set; }
    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// Gets the date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    #region Navigation properties
    /// <summary>
    /// Login history records associated with the user.
    /// </summary>
    private readonly List<LoginHistory> _loginHistories = [];
    /// <summary>
    /// Gets the collection of login history records associated with the user.
    /// </summary>
    public IReadOnlyCollection<LoginHistory> LoginHistories => _loginHistories.AsReadOnly();

    /// <summary>
    /// Refresh tokens associated with the user.
    /// </summary>
    private readonly List<RefreshToken> _refreshTokens = [];
    /// <summary>
    /// Gets the collection of refresh tokens associated with the user.
    /// </summary>
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    /// <summary>
    /// User roles assigned to the user.
    /// </summary>
    private readonly List<UserRole> _userRoles = [];
    /// <summary>
    /// Gets the collection of roles assigned to the user.
    /// </summary>
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    /// <summary>
    /// User attributes associated with the user.
    /// </summary>
    private readonly List<UserAttribute> _userAttributes = [];
    /// <summary>
    /// Gets the collection of attributes associated with the user.
    /// </summary>
    public IReadOnlyCollection<UserAttribute> UserAttributes => _userAttributes.AsReadOnly();

    /// <summary>
    /// Relationships with other users.
    /// </summary>
    private readonly List<UserRelationship> _relationships = [];
    /// <summary>
    /// Gets the collection of relationships associated with the user.
    /// </summary>
    public IReadOnlyCollection<UserRelationship> Relationships => _relationships.AsReadOnly();
    #endregion

    /// <summary>
    /// Initializes a new instance of the User class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core to materialize objects from the
    /// database. It should not be called directly in application code.</remarks>
    private User() { }

    /// <summary>
    /// Initializes a new instance of the User class with the specified email address, password, and full name.
    /// </summary>
    /// <param name="email">The email address associated with the user. Cannot be null.</param>
    /// <param name="password">The password for the user account. Cannot be null.</param>
    /// <param name="fullName">The full name of the user. Cannot be null.</param>
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

    /// <summary>
    /// Creates a new user with the specified email address, password, and full name.
    /// </summary>
    /// <remarks>This method also registers a domain event indicating that a new user has been created. The
    /// returned result will always contain a user instance if successful.</remarks>
    /// <param name="email">The email address to associate with the new user. Cannot be null.</param>
    /// <param name="password">The password to assign to the new user. Cannot be null.</param>
    /// <param name="fullName">The full name of the new user. Cannot be null.</param>
    /// <returns>A result containing the newly created user if the operation succeeds; otherwise, a result indicating failure.</returns>
    public static Result<User> Create(Email email, Password password, FullName fullName)
    {
        // Create the user instance
        var user = new User(email, password, fullName);
        // Register the domain event
        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email));
        // Return the successful result
        return Result.Success(user);
    }

    /// <summary>
    /// Updates the user's password to the specified value and records the change event.
    /// </summary>
    /// <remarks>This method updates the user's password and sets the update timestamp to the current UTC
    /// time. It also raises a domain event to signal that the user's password has changed.</remarks>
    /// <param name="newPassword">The new password to assign to the user. Cannot be null.</param>
    /// <returns>A result indicating whether the password update operation was successful.</returns>
    public Result UpdatePassword(Password newPassword)
    {
        // Update the password
        Password = newPassword;
        UpdatedAt = DateTime.UtcNow;
        // Raise the domain event
        AddDomainEvent(new UserPasswordChangedEvent(Id));
        // Return success
        return Result.Success();
    }

    /// <summary>
    /// Updates the user's profile with the specified full name.
    /// </summary>
    /// <param name="fullName">The new full name to assign to the user's profile. Cannot be null.</param>
    /// <returns>A Result object indicating whether the profile update was successful.</returns>
    public Result UpdateProfile(FullName fullName)
    {
        FullName = fullName;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    /// <summary>
    /// Marks the user's email address as verified if it has not already been verified.
    /// </summary>
    /// <remarks>This method updates the verification status and records the time of verification. It also
    /// raises a domain event when the email is successfully verified.</remarks>
    /// <returns>A <see cref="Result"/> indicating whether the email verification was successful. Returns a failure result if the
    /// email is already verified; otherwise, returns a success result.</returns>
    public Result VerifyEmail()
    {
        // Check if the email is already verified
        if (IsEmailVerified)
        {
            // Return failure if already verified
            return Result.Failure("Email is already verified.");
        }

        // Mark the email as verified
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
        // Raise the domain event
        AddDomainEvent(new UserEmailVerifiedEvent(Id));
        // Return success
        return Result.Success();
    }

    /// <summary>
    /// Records a successful login attempt for the user and updates the user's login state and history.
    /// </summary>
    /// <remarks>This method resets failed login attempts and unlocks the user account if it was previously
    /// locked. It also records the login event in the user's login history.</remarks>
    /// <param name="ipAddress">The IP address from which the user logged in. Cannot be null or empty.</param>
    /// <param name="userAgent">The user agent string associated with the login attempt. Cannot be null or empty.</param>
    /// <returns>A Result indicating whether the login was recorded successfully. Returns a failure result if the user account is
    /// not active or is currently locked.</returns>
    public Result RecordSuccessfulLogin(string ipAddress, string userAgent)
    {
        // Check if the user account is active
        if (!IsActive)
        {
            // Return failure if not active
            return Result.Failure("User account is not active.");
        }
        // Check if the user account is locked
        if (IsLocked && LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow)
        {
            // Return failure if locked
            return Result.Failure($"Account is locked until {LockedUntil.Value}.");
        }

        // Reset failed login attempts and unlock the account
        FailedLoginAttempts = 0;
        IsLocked = false;
        LockedUntil = null;
        LastLoginAt = DateTime.UtcNow;

        // Record the login history
        var loginHistory = LoginHistory.Create(Id, ipAddress, userAgent, true);
        // Add to the login histories
        _loginHistories.Add(loginHistory);

        // Raise the domain event
        AddDomainEvent(new UserLoggedInEvent(Id, ipAddress));
        // Return success
        return Result.Success();
    }

    /// <summary>
    /// Records a failed login attempt for the user and updates the account's lockout status if necessary.
    /// </summary>
    /// <remarks>If the number of consecutive failed login attempts reaches the configured threshold, the
    /// account is locked for a specified duration. Subsequent failed attempts while the account is locked will not
    /// reset the lockout timer.</remarks>
    /// <param name="ipAddress">The IP address from which the failed login attempt originated. Cannot be null or empty.</param>
    /// <param name="userAgent">The user agent string associated with the failed login attempt. Cannot be null or empty.</param>
    /// <returns>A failure result indicating the outcome of the login attempt. Returns a result with an error message if the
    /// account is locked due to multiple failed attempts.</returns>
    public Result RecordFailedLogin(string ipAddress, string userAgent)
    {
        // Check if the user account is active
        FailedLoginAttempts++;

        // Record the login history
        var loginHistory = LoginHistory.Create(Id, ipAddress, userAgent, false);
        // Add to the login histories
        _loginHistories.Add(loginHistory);

        // Check if the account should be locked
        if (FailedLoginAttempts >= 5)
        {
            // Lock the account for 30 minutes
            IsLocked = true;
            LockedUntil = DateTime.UtcNow.AddMinutes(30);
            // Raise the domain event
            AddDomainEvent(new UserLockedEvent(Id, LockedUntil.Value));
            // Return failure indicating account lock
            return Result.Failure("Account has been locked due to multiple failed login attempts.");
        }

        // Return failure indicating invalid credentials
        return Result.Failure("Invalid credentials.");
    }

    /// <summary>
    /// Creates and adds a new refresh token for the current user with the specified token value and expiration time.
    /// </summary>
    /// <param name="token">The token string to associate with the new refresh token. Cannot be null or empty.</param>
    /// <param name="expiresAt">The date and time when the refresh token expires.</param>
    /// <returns>A <see cref="RefreshToken"/> instance representing the newly created refresh token.</returns>
    public RefreshToken CreateRefreshToken(string token, DateTime expiresAt)
    {
        // Create the refresh token
        var refreshToken = RefreshToken.Create(Id, token, expiresAt);
        // Add to the collection
        _refreshTokens.Add(refreshToken);
        // Return the created token
        return refreshToken;
    }

    /// <summary>
    /// Revokes a refresh token with the specified identifier.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the refresh token to revoke.</param>
    /// <returns>A <see cref="Result"/> indicating whether the operation succeeded. Returns a failure result if the refresh token
    /// is not found.</returns>
    public Result RevokeRefreshToken(Guid tokenId)
    {
        // Find the refresh token by its ID
        var token = _refreshTokens.FirstOrDefault(t => t.Id == tokenId);
        if (token == null)
        {
            // Return failure if not found
            return Result.Failure("Refresh token not found.");
        }

        // Revoke the token
        token.Revoke();
        // Return success
        return Result.Success();
    }

    /// <summary>
    /// Assigns the specified role to the user if it is not already assigned.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to assign to the user.</param>
    public void AddRole(Guid roleId)
    {
        // Check if the role is already assigned
        if (_userRoles.Any(ur => ur.RoleId == roleId))
        {
            // Return if already assigned
            return;
        }

        // Create and add the user role
        var userRole = UserRole.Create(Id, roleId);
        // Add to the collection
        _userRoles.Add(userRole);
        // Raise the domain event
        AddDomainEvent(new UserRoleAssignedEvent(Id, roleId));
    }

    /// <summary>
    /// Removes the association between the user and the specified role, if it exists.
    /// </summary>
    /// <remarks>If the user is not associated with the specified role, this method has no effect. Removing a
    /// role triggers a domain event to indicate that the user's roles have changed.</remarks>
    /// <param name="roleId">The unique identifier of the role to remove from the user.</param>
    public void RemoveRole(Guid roleId)
    {
        // Find the user role association
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            // Remove the association
            _userRoles.Remove(userRole);
            // Raise the domain event
            AddDomainEvent(new UserRoleRemovedEvent(Id, roleId));
        }
    }

    /// <summary>
    /// Adds a user attribute with the specified key, value, and data type. If an attribute with the same key already
    /// exists, its value is updated.
    /// </summary>
    /// <remarks>If an attribute with the specified key already exists, only its value is updated; the data
    /// type is not changed. The method updates the last modified timestamp for the user.</remarks>
    /// <param name="key">The unique key that identifies the user attribute. Cannot be null or empty.</param>
    /// <param name="value">The value to assign to the user attribute. Cannot be null.</param>
    /// <param name="dataType">The data type of the attribute value. Defaults to "string" if not specified.</param>
    public void AddAttribute(string key, string value, string dataType = "string")
    {
        // Check if the attribute already exists
        var existingAttr = _userAttributes.FirstOrDefault(a => a.AttributeKey == key);
        if (existingAttr != null)
        {
            // Update the existing attribute's value
            existingAttr.UpdateValue(value);
        }
        else
        {
            // Create and add the new attribute
            var attribute = UserAttribute.Create(Id, key, value, dataType);
            // Add to the collection
            _userAttributes.Add(attribute);
        }

        // Update the timestamp
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a relationship of the specified type to the target user if it does not already exist.
    /// </summary>
    /// <remarks>If a relationship of the specified type with the target user already exists, this method does
    /// nothing.</remarks>
    /// <param name="targetUserId">The unique identifier of the user to establish the relationship with.</param>
    /// <param name="relationshipType">The type of relationship to add. This value determines the nature of the association between users.</param>
    public void AddRelationship(Guid targetUserId, string relationshipType)
    {
        // Check if the relationship already exists
        if (_relationships.Any(r => r.TargetUserId == targetUserId && r.RelationshipType == relationshipType))
        {
            // Return if already exists
            return;
        }

        // Create and add the relationship
        var relationship = UserRelationship.Create(Id, targetUserId, relationshipType);
        // Add to the collection
        _relationships.Add(relationship);
    }

    /// <summary>
    /// Deactivates the user and records the deactivation event.
    /// </summary>
    /// <remarks>This method sets the user as inactive and updates the last modified timestamp. A domain event
    /// is raised to signal the deactivation, which may trigger additional actions in the system.</remarks>
    /// <returns>A <see cref="Result"/> indicating whether the deactivation was successful.</returns>
    public Result Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        // Raise the domain event
        AddDomainEvent(new UserDeactivatedEvent(Id));
        // Return success
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
        // Raise the domain event
        return Result.Success();
    }
}
