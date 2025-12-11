using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolcanionAuth.Application.Features.Authentication.Commands.LoginUser;
using VolcanionAuth.Application.Features.Authentication.Commands.RefreshToken;
using VolcanionAuth.Application.Features.Authentication.Commands.RegisterUser;

namespace VolcanionAuth.API.Controllers.V1;

/// <summary>
/// Provides API endpoints for user authentication operations, including registration, login, and logout.
/// </summary>
/// <remarks>This controller exposes authentication-related actions for clients, such as registering new users,
/// logging in, and logging out. All endpoints are versioned and follow RESTful conventions. Registration and login
/// endpoints allow anonymous access, while logout requires authentication. Responses include appropriate status codes
/// and error information when applicable.</remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthenticationController(IMediator mediator, ILogger<AuthenticationController> logger) : ControllerBase
{
    /// <summary>
    /// Handles a user registration request by creating a new user account with the provided registration details.
    /// </summary>
    /// <remarks>Returns HTTP 200 OK with user details on successful registration, or HTTP 400 Bad Request if
    /// registration fails due to validation or business rule errors. This endpoint is accessible without
    /// authentication.</remarks>
    /// <param name="command">The registration information for the new user, including required fields such as email and password. Cannot be
    /// null.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="RegisterUserResponse"/> with user details if registration
    /// succeeds; otherwise, a Bad Request response with error information.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        // Log the registration attempt
        logger.LogDebug("User registration attempt for email: {Email}", command.Email);

        // Send the registration command to the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Log the failure reason
            logger.LogWarning("User registration failed: {Error}", result.Error);
            // Return a Bad Request response with the error details
            return BadRequest(new { error = result.Error });
        }

        // Log the successful registration
        logger.LogDebug("User registered successfully: {UserId}", result.Value.UserId);
        // Return an OK response with the registered user details
        return Ok(result.Value);
    }

    /// <summary>
    /// Authenticates a user using the provided login credentials and returns a response indicating the result of the
    /// login attempt.
    /// </summary>
    /// <remarks>This endpoint is accessible without authentication. The user's IP address and user agent are
    /// automatically included in the login request for auditing and security purposes. Returns HTTP 200 (OK) on success
    /// or HTTP 401 (Unauthorized) if authentication fails.</remarks>
    /// <param name="command">The login request containing user credentials and any additional information required for authentication. Cannot
    /// be null.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="LoginUserResponse"/> if authentication is successful;
    /// otherwise, an unauthorized response with error details.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        // Extract IP address and User-Agent from the request
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        // In case User-Agent header is missing, default to "Unknown"
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        // Log the login attempt
        var loginCommand = command with { IpAddress = ipAddress, UserAgent = userAgent };

        // Send the login command to the mediator
        var result = await mediator.Send(loginCommand);
        if (result.IsFailure)
        {
            // Log the failed login attempt
            logger.LogWarning("Login failed for email: {Email}", command.Email);
            // Return an Unauthorized response with the error details
            return Unauthorized(new { error = result.Error });
        }

        // Log the successful login
        logger.LogDebug("User logged in successfully: {UserId}", result.Value.UserId);
        // Return an OK response with the login details
        return Ok(result.Value);
    }

    /// <summary>
    /// Logs out the currently authenticated user and returns a success response.
    /// </summary>
    /// <returns>An <see cref="OkObjectResult"/> containing a message indicating that the user has been logged out successfully.</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        // Log the logout action
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        // In a real application, you might want to invalidate tokens or clear session data here
        logger.LogDebug("User logged out: {UserId}", userId);
        // Return an OK response indicating successful logout
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Refreshes an access token using a valid refresh token.
    /// </summary>
    /// <remarks>This endpoint allows clients to obtain a new access token and refresh token pair using a valid
    /// refresh token, without requiring the user to re-authenticate. Returns HTTP 200 OK with new tokens on success,
    /// or HTTP 400 Bad Request if the refresh token is invalid, expired, or revoked.</remarks>
    /// <param name="command">The refresh token request containing the refresh token to validate.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="RefreshTokenResponse"/> with new access and refresh
    /// tokens if successful; otherwise, a Bad Request response with error information.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        // Log the refresh token attempt
        logger.LogDebug("Token refresh attempt");

        // Send the refresh token command to the mediator
        var result = await mediator.Send(command);
        if (result.IsFailure)
        {
            // Log the failure reason
            logger.LogWarning("Token refresh failed: {Error}", result.Error);
            // Return a Bad Request response with the error details
            return BadRequest(new { error = result.Error });
        }

        // Log the successful token refresh
        logger.LogDebug("Token refreshed successfully");
        // Return an OK response with the new tokens
        return Ok(result.Value);
    }
}
