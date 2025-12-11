using VolcanionAuth.Domain.Common;

namespace VolcanionAuth.Domain.Entities;

/// <summary>
/// Represents a record of a user's login attempt, including details such as the user, IP address, user agent,
/// timestamp, and outcome.
/// </summary>
/// <remarks>A LoginHistory instance captures information about each login attempt for auditing and security
/// purposes. It includes both successful and failed login attempts, along with optional failure reasons. This type is
/// typically used to track authentication activity and support features such as security monitoring or user activity
/// history.</remarks>
public class LoginHistory : Entity<Guid>
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public Guid UserId { get; private set; }
    /// <summary>
    /// Gets the IP address associated with the current instance.
    /// </summary>
    public string IpAddress { get; private set; } = null!;
    /// <summary>
    /// Gets the value of the HTTP User-Agent header sent with requests.
    /// </summary>
    public string UserAgent { get; private set; } = null!;
    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccessful { get; private set; }
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; private set; }
    /// <summary>
    /// Gets the reason for the failure, if the operation did not succeed.
    /// </summary>
    public string? FailureReason { get; private set; }

    #region Navigation
    /// <summary>
    /// Gets the user associated with the current context.
    /// </summary>
    public User User { get; private set; } = null!;
    #endregion

    /// <summary>
    /// Initializes a new instance of the LoginHistory class for use by Entity Framework Core.
    /// </summary>
    /// <remarks>This constructor is intended for use by Entity Framework Core to support materialization of
    /// LoginHistory entities. It should not be called directly in application code.</remarks>
    private LoginHistory() { }

    /// <summary>
    /// Initializes a new instance of the LoginHistory class with the specified user information and login result.
    /// </summary>
    /// <param name="userId">The unique identifier of the user associated with the login attempt.</param>
    /// <param name="ipAddress">The IP address from which the login attempt was made.</param>
    /// <param name="userAgent">The user agent string of the client used during the login attempt.</param>
    /// <param name="isSuccessful">A value indicating whether the login attempt was successful. Set to <see langword="true"/> if the login
    /// succeeded; otherwise, <see langword="false"/>.</param>
    /// <param name="failureReason">The reason for a failed login attempt, or <see langword="null"/> if the login was successful.</param>
    private LoginHistory(Guid userId, string ipAddress, string userAgent, bool isSuccessful, string? failureReason = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IsSuccessful = isSuccessful;
        FailureReason = failureReason;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new instance of the LoginHistory class with the specified user and login details.
    /// </summary>
    /// <param name="userId">The unique identifier of the user associated with the login attempt.</param>
    /// <param name="ipAddress">The IP address from which the login attempt was made. Cannot be null or empty.</param>
    /// <param name="userAgent">The user agent string of the client used during the login attempt. Cannot be null or empty.</param>
    /// <param name="isSuccessful">A value indicating whether the login attempt was successful. Set to <see langword="true"/> if the login
    /// succeeded; otherwise, <see langword="false"/>.</param>
    /// <param name="failureReason">The reason for a failed login attempt, or <see langword="null"/> if the login was successful.</param>
    /// <returns>A new LoginHistory object containing the details of the login attempt.</returns>
    public static LoginHistory Create(Guid userId, string ipAddress, string userAgent, bool isSuccessful, string? failureReason = null)
    {
        // Input validation
        return new LoginHistory(userId, ipAddress, userAgent, isSuccessful, failureReason);
    }
}
