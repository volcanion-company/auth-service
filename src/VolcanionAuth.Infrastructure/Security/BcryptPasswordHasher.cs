using VolcanionAuth.Application.Common.Interfaces;

namespace VolcanionAuth.Infrastructure.Security;

/// <summary>
/// Provides methods for hashing and verifying passwords using the bcrypt algorithm.
/// </summary>
/// <remarks>This implementation uses the bcrypt algorithm to securely hash passwords and verify password hashes.
/// Bcrypt is designed to be computationally intensive to help protect against brute-force attacks. The default work
/// factor is set to 12, which balances security and performance for most applications. This class is typically used to
/// store password hashes and to verify user credentials during authentication.</remarks>
public class BcryptPasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Generates a secure hash for the specified password using the BCrypt algorithm.
    /// </summary>
    /// <remarks>The generated hash includes a salt and can be stored for later password verification. The
    /// BCrypt algorithm is designed to be computationally intensive to help protect against brute-force
    /// attacks.</remarks>
    /// <param name="password">The plain text password to hash. Cannot be null or empty.</param>
    /// <returns>A string containing the hashed representation of the password.</returns>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifies whether the specified password matches the provided BCrypt hash.
    /// </summary>
    /// <remarks>This method returns false if the hash is invalid or if verification fails for any
    /// reason.</remarks>
    /// <param name="password">The plain text password to verify. Cannot be null.</param>
    /// <param name="hash">The BCrypt hash to compare against. Cannot be null.</param>
    /// <returns>true if the password matches the hash; otherwise, false.</returns>
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            // Use BCrypt to verify the password against the hash
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // In case of any exception (e.g., invalid hash), return false
            return false;
        }
    }
}
