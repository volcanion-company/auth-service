using BCrypt.Net;
using VolcanionAuth.Application.Common.Interfaces;

namespace VolcanionAuth.Infrastructure.Security;

/// <summary>
/// BCrypt password hasher implementation
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
