namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// Defines methods for hashing passwords and verifying password hashes.
/// </summary>
/// <remarks>Implementations of this interface should use secure, industry-standard hashing algorithms to protect
/// password data. The interface does not specify the hashing algorithm or storage format, allowing flexibility for
/// different security requirements. It is recommended to use a unique salt for each password and to avoid storing plain
/// text passwords.</remarks>
public interface IPasswordHasher
{
    /// <summary>
    /// Generates a secure hash for the specified password using a cryptographic algorithm.
    /// </summary>
    /// <param name="password">The plain text password to be hashed. Cannot be null or empty.</param>
    /// <returns>A string containing the hashed representation of the password. The format and encoding of the hash may vary
    /// depending on the implementation.</returns>
    string HashPassword(string password);
    /// <summary>
    /// Verifies whether the specified password matches the provided password hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify against the hash. Cannot be null.</param>
    /// <param name="hash">The hashed password to compare with. Cannot be null.</param>
    /// <returns>true if the password matches the hash; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash);
}
