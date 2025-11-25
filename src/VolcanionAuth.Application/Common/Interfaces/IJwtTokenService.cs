using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Common.Interfaces;

/// <summary>
/// JWT token service interface
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default);
}
