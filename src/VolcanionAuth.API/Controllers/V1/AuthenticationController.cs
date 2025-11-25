using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.Application.Features.Authentication.Commands.LoginUser;
using VolcanionAuth.Application.Features.Authentication.Commands.RegisterUser;

namespace VolcanionAuth.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IMediator mediator, ILogger<AuthenticationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        _logger.LogInformation("User registration attempt for email: {Email}", command.Email);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("User registration failed: {Error}", result.Error);
            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("User registered successfully: {UserId}", result.Value.UserId);
        return Ok(result.Value);
    }

    /// <summary>
    /// Login user
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var loginCommand = command with { IpAddress = ipAddress, UserAgent = userAgent };
        var result = await _mediator.Send(loginCommand);

        if (result.IsFailure)
        {
            _logger.LogWarning("Login failed for email: {Email}", command.Email);
            return Unauthorized(new { error = result.Error });
        }

        _logger.LogInformation("User logged in successfully: {UserId}", result.Value.UserId);
        return Ok(result.Value);
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User logged out: {UserId}", userId);
        
        return Ok(new { message = "Logged out successfully" });
    }
}
