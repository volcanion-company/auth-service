using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Defines methods for generating, validating, and extracting information from JSON Web Tokens (JWTs) used for
/// authentication and authorization.
/// </summary>
/// <remarks>Implementations of this interface typically provide functionality for issuing access and refresh
/// tokens, validating token integrity and expiration, and retrieving user identifiers from tokens. These methods are
/// commonly used in authentication workflows to manage user sessions and permissions.</remarks>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a secure access token for the specified user, including the provided roles and permissions.
    /// </summary>
    /// <remarks>The generated token can be used for authenticating and authorizing API requests. Ensure that
    /// the roles and permissions accurately reflect the user's access rights, as they will be trusted by downstream
    /// services.</remarks>
    /// <param name="user">The user for whom the access token is generated. Cannot be null.</param>
    /// <param name="roles">A collection of role names to be embedded in the token. Each role should represent an authorization level or
    /// group assigned to the user.</param>
    /// <param name="permissions">A collection of permission strings to be embedded in the token. Each permission should specify an action or
    /// resource the user is allowed to access.</param>
    /// <returns>A string containing the generated access token. The token encodes the user's identity, roles, and permissions.</returns>
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    /// <summary>
    /// Generates a new refresh token for use in authentication workflows.
    /// </summary>
    /// <returns>A string containing the newly generated refresh token. The token is suitable for use in subsequent
    /// authentication requests.</returns>
    string GenerateRefreshToken();
    /// <summary>
    /// Asynchronously validates the specified authentication token.
    /// </summary>
    /// <param name="token">The token to validate. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the validation operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the token is
    /// valid; otherwise, <see langword="false"/>.</returns>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    /// <summary>
    /// Asynchronously retrieves the user identifier associated with the specified authentication token.
    /// </summary>
    /// <param name="token">The authentication token to validate and use for identifying the user. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Guid"/> representing the user identifier if the token is valid; otherwise, <see langword="null"/>.</returns>
    Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default);
}
