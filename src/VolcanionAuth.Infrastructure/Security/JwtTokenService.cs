using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Infrastructure.Security;

/// <summary>
/// Provides functionality for generating, validating, and extracting information from JSON Web Tokens (JWT) for
/// authentication and authorization purposes.
/// </summary>
/// <remarks>This service is typically used in authentication workflows to issue access and refresh tokens,
/// validate incoming JWTs, and extract user identifiers from tokens. The configuration must include valid JWT settings;
/// otherwise, initialization may fail. The service is not thread stateful and can be safely used across multiple
/// requests.</remarks>
/// <param name="configuration">The application configuration instance used to retrieve JWT settings such as secret key, issuer, audience, and token
/// expiration.</param>
public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    /// <summary>
    /// Secret key used for signing JWTs.
    /// </summary>
    private readonly string _secretKey = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
    /// <summary>
    /// Issuer of the JWTs.
    /// </summary>
    private readonly string _issuer = configuration["JwtSettings:Issuer"] ?? "VolcanionAuth";
    /// <summary>
    /// Audience for the JWTs.
    /// </summary>
    private readonly string _audience = configuration["JwtSettings:Audience"] ?? "VolcanionAuthAPI";
    /// <summary>
    /// Access token expiration time in minutes.
    /// </summary>
    private readonly int _accessTokenExpirationMinutes = int.Parse(configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "30");

    /// <summary>
    /// Generates a JSON Web Token (JWT) access token for the specified user, including the provided roles and
    /// permissions as claims.
    /// </summary>
    /// <remarks>The generated token includes standard claims such as subject, email, and a unique identifier,
    /// as well as custom claims for roles and permissions. The token is signed using the configured secret key and is
    /// valid for a limited duration as specified by the access token expiration setting.</remarks>
    /// <param name="user">The user for whom the access token is generated. Cannot be null.</param>
    /// <param name="roles">A collection of role names to include as role claims in the token. Each role will be added as a separate claim.</param>
    /// <param name="permissions">A collection of permission strings to include as permission claims in the token. Each permission will be added
    /// as a separate claim.</param>
    /// <returns>A JWT access token as a string, containing claims for the user, roles, and permissions.</returns>
    public string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        // Define token claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),           // Subject - User ID
            new(JwtRegisteredClaimNames.Email, user.Email.Value),           // Email
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),    // Unique Identifier for the token
            new(ClaimTypes.Name, user.FullName.GetFullName()),              // User's full name
            new("email_verified", user.IsEmailVerified.ToString()),         // Email verified status
        };

        // Add role claims
        foreach (var role in roles)
        {
            // Add each role as a separate claim
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        // Add permission claims
        foreach (var permission in permissions)
        {
            // Add each permission as a separate claim
            claims.Add(new Claim("permission", permission));
        }

        // Create signing credentials using the secret key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        // Create signing credentials
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create the JWT token
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        // Return the serialized token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure random refresh token as a Base64-encoded string.
    /// </summary>
    /// <remarks>The generated token is suitable for use in authentication scenarios where a high-entropy,
    /// unpredictable value is required, such as OAuth or JWT refresh tokens.</remarks>
    /// <returns>A Base64-encoded string representing a securely generated refresh token.</returns>
    public string GenerateRefreshToken()
    {
        // Generate a secure random 64-byte token
        var randomNumber = new byte[64];
        // Use a cryptographic random number generator
        using var rng = RandomNumberGenerator.Create();
        // Fill the byte array with random bytes
        rng.GetBytes(randomNumber);
        // Return the Base64-encoded string representation of the token
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Asynchronously validates a JSON Web Token (JWT) against the configured issuer, audience, and signing key.
    /// </summary>
    /// <remarks>The validation checks the token's signature, issuer, audience, and expiration. If the token
    /// is invalid or expired, the method returns <see langword="false"/>. No exceptions are thrown for invalid tokens;
    /// callers should check the return value to determine validity.</remarks>
    /// <param name="token">The JWT to validate. Must be a non-empty, well-formed token string.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the token is
    /// valid; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // Create a token handler
        var tokenHandler = new JwtSecurityTokenHandler();
        // Convert the secret key to bytes
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            // Validate the token
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,                    // Validate the signing key
                IssuerSigningKey = new SymmetricSecurityKey(key),   // Signing key
                ValidateIssuer = true,                              // Validate the issuer
                ValidIssuer = _issuer,                              // Expected issuer
                ValidateAudience = true,                            // Validate the audience
                ValidAudience = _audience,                          // Expected audience
                ValidateLifetime = true,                            // Validate token expiration
                ClockSkew = TimeSpan.Zero,                          // No clock skew
            }, out _);

            // 
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Asynchronously extracts the user identifier from a JWT access token.
    /// </summary>
    /// <remarks>If the token is invalid, does not contain a 'sub' claim, or the 'sub' claim is not a valid
    /// GUID, the method returns <see langword="null"/>. No exceptions are thrown for invalid tokens.</remarks>
    /// <param name="token">The JWT access token from which to extract the user identifier. Must be a valid JWT containing a 'sub' (subject)
    /// claim representing the user ID as a GUID.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Guid"/> representing the user identifier if the token contains a valid 'sub' claim; otherwise, <see
    /// langword="null"/>.</returns>
    public async Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create a token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            // Read the JWT token
            var jwtToken = tokenHandler.ReadJwtToken(token);
            //  Find the 'sub' claim
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                // Return the parsed user ID
                return userId;
            }

            // 'sub' claim not found or invalid
            return null;
        }
        catch
        {
            // In case of any exception (e.g., invalid token), return null
            return null;
        }
    }
}
