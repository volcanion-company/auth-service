using MediatR;
using VolcanionAuth.Application.Common.Interfaces;
using VolcanionAuth.Domain.Common;
using VolcanionAuth.Domain.Entities;

namespace VolcanionAuth.Application.Features.Authentication.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IReadRepository<User> _readRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public LoginUserCommandHandler(
        IRepository<User> userRepository,
        IReadRepository<User> readRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _readRepository = readRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        // Get user with roles and permissions
        var user = await _readRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            return Result.Failure<LoginUserResponse>("Invalid credentials.");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.Password.Hash))
        {
            user.RecordFailedLogin(request.IpAddress, request.UserAgent);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<LoginUserResponse>("Invalid credentials.");
        }

        // Record successful login
        var loginResult = user.RecordSuccessfulLogin(request.IpAddress, request.UserAgent);
        if (loginResult.IsFailure)
            return Result.Failure<LoginUserResponse>(loginResult.Error);

        // Get user permissions
        var permissions = await _readRepository.GetUserPermissionsAsync(user.Id, cancellationToken);
        var roles = user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList();
        var permissionStrings = permissions.Select(p => p.GetPermissionString()).ToList();

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, permissionStrings);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Create refresh token
        user.CreateRefreshToken(refreshToken, expiresAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache user session
        await _cacheService.SetAsync($"user_session:{user.Id}", new
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
