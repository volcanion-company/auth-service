using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents a refresh token used to obtain new access tokens for a user in authentication workflows.
/// </summary>
/// <remarks>A refresh token is typically issued alongside an access token and allows clients to request new
/// access tokens without requiring the user to re-authenticate. This class tracks the token's creation, expiration,
/// revocation status, and the associated user.</remarks>
public class RefreshToken : Entity<Guid>
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid UserId { get; private set; }
    /// <summary>
    /// Gets the authentication token used to authorize requests.
    /// </summary>
    public string Token { get; private set; } = null!;
    /// <summary>
    /// Gets the date and time when the current object expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }
    /// <summary>
    /// Gets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    /// <summary>
    /// Gets the date and time when the entity was revoked, if applicable.
    /// </summary>
    public DateTime? RevokedAt { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the entity has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;
    /// <summary>
    /// Gets a value indicating whether the current object has expired based on its expiration time.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    /// <summary>
    /// Gets a value indicating whether the item is currently active.
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Gets the user associated with the current context.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the RefreshToken class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core when materializing objects from
    /// a database. It should not be called directly in application code.</remarks>
    private RefreshToken() { }

    /// <summary>
    /// Initializes a new instance of the RefreshToken class for the specified user with the given token and expiration
    /// time.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to whom the refresh token is associated.</param>
    /// <param name="token">The refresh token string to be assigned to the user.</param>
    /// <param name="expiresAt">The date and time, in UTC, when the refresh token expires.</param>
    private RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new instance of the RefreshToken class for the specified user with the given token and expiration
    /// time.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to whom the refresh token will be associated.</param>
    /// <param name="token">The refresh token string to assign to the user.</param>
    /// <param name="expiresAt">The date and time when the refresh token will expire.</param>
    /// <returns>A new RefreshToken instance initialized with the specified user ID, token, and expiration time.</returns>
    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        // TODO: You might want to add validation logic here in the future.
        return new RefreshToken(userId, token, expiresAt);
    }

    /// <summary>
    /// Revokes the current entity by marking it as revoked at the current UTC time.
    /// </summary>
    /// <remarks>After calling this method, the entity is considered revoked. Subsequent operations may check
    /// the revocation status to determine if the entity is valid for use.</remarks>
    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}
