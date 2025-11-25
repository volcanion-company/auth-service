using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.Authentication.Commands.LoginUser;

public class LoginUserCommandHandler(
    IUserRepository userRepository,
    IReadRepository<User> readRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // Normalize email for case-insensitive comparison
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        
        // Get user from write repository with roles (tracked entity)
        var user = await userRepository.GetUserByEmailWithRolesAsync(normalizedEmail, cancellationToken);
        
        if (user == null)
        {
            return Result.Failure<LoginUserResponse>("Invalid credentials.");
        }

        // Verify password
        if (!passwordHasher.VerifyPassword(request.Password, user.Password.Hash))
        {
            user.RecordFailedLogin(request.IpAddress, request.UserAgent);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<LoginUserResponse>("Invalid credentials.");
        }

        // Record successful login
        var loginResult = user.RecordSuccessfulLogin(request.IpAddress, request.UserAgent);
        if (loginResult.IsFailure)
        {
            return Result.Failure<LoginUserResponse>(loginResult.Error);
        }

        // Get user permissions from read repository
        var permissions = await readRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
        
        // Get roles from the tracked user entity (already loaded with Include)
        var roles = user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList();
        var permissionStrings = permissions.Select(p => p.GetPermissionString()).ToList();

        // Generate tokens
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles, permissionStrings);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Create refresh token
        user.CreateRefreshToken(refreshToken, expiresAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache user session
        await cacheService.SetAsync($"user_session:{user.Id}", new
        {
            user.Id,
            user.Email,
            Roles = roles,
            Permissions = permissionStrings
        }, TimeSpan.FromMinutes(30), cancellationToken);

        var response = new LoginUserResponse(
            accessToken,
            refreshToken,
            expiresAt,
            user.Id,
            user.Email,
            user.FullName.GetFullName()
        );

        return Result.Success(response);
    }
}
