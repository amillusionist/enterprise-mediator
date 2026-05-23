using System.Net.Mime;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Emp.ApiGateway.Web.Controllers;

/// <summary>
/// Handles authentication operations via AWS Cognito.
/// Provides login, registration, token refresh, MFA verification, and password management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController : ControllerBase
{
    private readonly ICognitoAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ICognitoAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Authentication tokens and user info, or MFA challenge.</returns>
    /// <response code="200">Login successful or MFA required.</response>
    /// <response code="401">Invalid credentials.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Login attempt for {Email}", request.Email);
        var response = await _authService.LoginAsync(request.Email, request.Password, ct);
        return Ok(response);
    }

    /// <summary>
    /// Signs out the current user by invalidating the access token.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Logout successful.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        await _authService.LogoutAsync(token, ct);
        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Completes user registration via an invitation token.
    /// </summary>
    /// <param name="request">Registration details including invite token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Authentication tokens and user info.</returns>
    /// <response code="201">Registration successful.</response>
    /// <response code="400">Invalid registration data or expired invite.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Registration attempt via invite token");
        var response = await _authService.RegisterAsync(request.InviteToken, request.Name, request.Password, ct);
        return CreatedAtAction(null, response);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>New authentication tokens.</returns>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken ?? string.Empty, ct);
        return Ok(response);
    }

    /// <summary>
    /// Verifies a multi-factor authentication code for a pending session.
    /// </summary>
    /// <param name="request">MFA session and verification code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Authentication tokens upon successful verification.</returns>
    /// <response code="200">MFA verification successful.</response>
    /// <response code="401">Invalid MFA code or session.</response>
    [HttpPost("mfa/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> VerifyMfa(
        [FromBody] MfaVerifyRequest request,
        CancellationToken ct)
    {
        var response = await _authService.VerifyMfaAsync(request.SessionId, request.Code, ct);
        return Ok(response);
    }

    /// <summary>
    /// Initiates the forgot password flow, sending a reset code to the user's email.
    /// Always returns 200 regardless of whether the email exists to prevent enumeration.
    /// </summary>
    /// <param name="request">The email address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Password reset initiated (if email exists).</response>
    [HttpPost("password/forgot")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        await _authService.ForgotPasswordAsync(request.Email, ct);
        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    /// <summary>
    /// Completes the password reset using a verification token and new password.
    /// </summary>
    /// <param name="request">Reset token and new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Password reset successfully.</response>
    /// <response code="400">Invalid token or password does not meet requirements.</response>
    [HttpPost("password/reset")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(request.Token, request.NewPassword, ct);
        return Ok(new { message = "Password reset successfully." });
    }
}

/// <summary>
/// Request payload for MFA verification.
/// </summary>
public record MfaVerifyRequest(string SessionId, string Code);

/// <summary>
/// Request payload for initiating a forgot password flow.
/// </summary>
public record ForgotPasswordRequest(string Email);

/// <summary>
/// Request payload for completing a password reset.
/// </summary>
public record ResetPasswordRequest(string Token, string NewPassword);
