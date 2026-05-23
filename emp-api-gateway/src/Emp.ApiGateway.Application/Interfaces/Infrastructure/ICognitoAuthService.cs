using EnterpriseMediator.Contracts.DTOs.Auth;

namespace Emp.ApiGateway.Application.Interfaces.Infrastructure;

/// <summary>
/// Contract for AWS Cognito authentication operations.
/// Handles login, registration, token refresh, MFA verification, and password management.
/// </summary>
public interface ICognitoAuthService
{
    /// <summary>
    /// Authenticates a user with email and password via AWS Cognito.
    /// </summary>
    Task<LoginResponse> LoginAsync(string email, string password, CancellationToken ct);

    /// <summary>
    /// Signs out the user by invalidating their access token in Cognito.
    /// </summary>
    Task LogoutAsync(string accessToken, CancellationToken ct);

    /// <summary>
    /// Completes user registration using an invitation token, setting name and password in Cognito.
    /// </summary>
    Task<LoginResponse> RegisterAsync(string inviteToken, string name, string password, CancellationToken ct);

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct);

    /// <summary>
    /// Verifies a multi-factor authentication code for a pending session.
    /// </summary>
    Task<LoginResponse> VerifyMfaAsync(string sessionId, string code, CancellationToken ct);

    /// <summary>
    /// Initiates the forgot password flow, sending a reset code to the user's email.
    /// </summary>
    Task ForgotPasswordAsync(string email, CancellationToken ct);

    /// <summary>
    /// Completes the password reset using a verification token and new password.
    /// </summary>
    Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct);
}
