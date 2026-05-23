using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using Emp.ApiGateway.Infrastructure.Configuration;
using EnterpriseMediator.Contracts.DTOs.Auth;
using EnterpriseMediator.Contracts.DTOs.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Emp.ApiGateway.Infrastructure.Services;

/// <summary>
/// AWS Cognito authentication service implementation.
/// Handles login, registration, token refresh, MFA verification, and password management
/// via the Amazon Cognito Identity Provider SDK.
/// </summary>
public class CognitoAuthService : ICognitoAuthService
{
    private readonly AmazonCognitoIdentityProviderClient _cognitoClient;
    private readonly IUserServiceClient _userServiceClient;
    private readonly AwsCognitoSettings _cognitoSettings;
    private readonly ILogger<CognitoAuthService> _logger;

    public CognitoAuthService(
        AmazonCognitoIdentityProviderClient cognitoClient,
        IUserServiceClient userServiceClient,
        IOptions<AwsCognitoSettings> cognitoSettings,
        ILogger<CognitoAuthService> logger)
    {
        _cognitoClient = cognitoClient ?? throw new ArgumentNullException(nameof(cognitoClient));
        _userServiceClient = userServiceClient ?? throw new ArgumentNullException(nameof(userServiceClient));
        _cognitoSettings = cognitoSettings?.Value ?? throw new ArgumentNullException(nameof(cognitoSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<LoginResponse> LoginAsync(string email, string password, CancellationToken ct)
    {
        _logger.LogInformation("Initiating Cognito authentication for user {Email}", email);

        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = _cognitoSettings.ClientId,
            AuthParameters = new Dictionary<string, string>
            {
                { "USERNAME", email },
                { "PASSWORD", password }
            }
        };

        var authResponse = await _cognitoClient.InitiateAuthAsync(authRequest, ct);

        if (authResponse.ChallengeName == ChallengeNameType.SMS_MFA ||
            authResponse.ChallengeName == ChallengeNameType.SOFTWARE_TOKEN_MFA)
        {
            _logger.LogInformation("MFA challenge required for user {Email}", email);

            return new LoginResponse
            {
                RequiresMfa = true,
                MfaSessionId = authResponse.Session,
                User = new UserInfo
                {
                    Id = string.Empty,
                    Email = email,
                    Name = string.Empty,
                    Role = string.Empty,
                    IsActive = true,
                    MfaEnabled = true
                },
                Tokens = new TokenInfo
                {
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty,
                    ExpiresIn = 0
                }
            };
        }

        var userProfile = await _userServiceClient.GetUserProfileByEmailAsync(email, ct);

        _logger.LogInformation("Authenticated user {Email} successfully", email);

        return BuildLoginResponse(authResponse.AuthenticationResult, userProfile);
    }

    /// <inheritdoc />
    public async Task LogoutAsync(string accessToken, CancellationToken ct)
    {
        _logger.LogInformation("Signing out user from Cognito");

        var request = new GlobalSignOutRequest
        {
            AccessToken = accessToken
        };

        await _cognitoClient.GlobalSignOutAsync(request, ct);

        _logger.LogInformation("User signed out successfully");
    }

    /// <inheritdoc />
    public async Task<LoginResponse> RegisterAsync(string inviteToken, string name, string password, CancellationToken ct)
    {
        _logger.LogInformation("Processing registration via invitation token");

        var invitation = await _userServiceClient.ValidateInvitationAsync(inviteToken, ct);

        if (invitation == null || !invitation.IsValid)
        {
            _logger.LogWarning("Invalid or expired invitation token used for registration");
            throw new InvalidOperationException("The invitation token is invalid or has expired.");
        }

        var signUpRequest = new SignUpRequest
        {
            ClientId = _cognitoSettings.ClientId,
            Username = invitation.Email,
            Password = password,
            UserAttributes =
            [
                new AttributeType { Name = "email", Value = invitation.Email },
                new AttributeType { Name = "name", Value = name },
                new AttributeType { Name = "email_verified", Value = "true" }
            ]
        };

        await _cognitoClient.SignUpAsync(signUpRequest, ct);

        var confirmRequest = new AdminConfirmSignUpRequest
        {
            UserPoolId = _cognitoSettings.UserPoolId,
            Username = invitation.Email
        };

        await _cognitoClient.AdminConfirmSignUpAsync(confirmRequest, ct);

        await _userServiceClient.ActivateUserAsync(inviteToken, new ActivateUserRequest
        {
            Password = password,
            FullName = name
        }, ct);

        _logger.LogInformation("User registered and activated successfully for email {Email}", invitation.Email);

        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = _cognitoSettings.ClientId,
            AuthParameters = new Dictionary<string, string>
            {
                { "USERNAME", invitation.Email },
                { "PASSWORD", password }
            }
        };

        var authResponse = await _cognitoClient.InitiateAuthAsync(authRequest, ct);
        var userProfile = await _userServiceClient.GetUserProfileByEmailAsync(invitation.Email, ct);

        return BuildLoginResponse(authResponse.AuthenticationResult, userProfile);
    }

    /// <inheritdoc />
    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        _logger.LogInformation("Refreshing access token via Cognito");

        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
            ClientId = _cognitoSettings.ClientId,
            AuthParameters = new Dictionary<string, string>
            {
                { "REFRESH_TOKEN", refreshToken }
            }
        };

        var authResponse = await _cognitoClient.InitiateAuthAsync(authRequest, ct);

        var getUserRequest = new GetUserRequest
        {
            AccessToken = authResponse.AuthenticationResult.AccessToken
        };

        var cognitoUser = await _cognitoClient.GetUserAsync(getUserRequest, ct);
        var email = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "email")?.Value ?? string.Empty;
        var userProfile = await _userServiceClient.GetUserProfileByEmailAsync(email, ct);

        _logger.LogInformation("Token refreshed successfully for user {Email}", email);

        return BuildLoginResponse(authResponse.AuthenticationResult, userProfile, refreshToken);
    }

    /// <inheritdoc />
    public async Task<LoginResponse> VerifyMfaAsync(string sessionId, string code, CancellationToken ct)
    {
        _logger.LogInformation("Verifying MFA code for session");

        var challengeRequest = new RespondToAuthChallengeRequest
        {
            ClientId = _cognitoSettings.ClientId,
            ChallengeName = ChallengeNameType.SOFTWARE_TOKEN_MFA,
            Session = sessionId,
            ChallengeResponses = new Dictionary<string, string>
            {
                { "SOFTWARE_TOKEN_MFA_CODE", code }
            }
        };

        var challengeResponse = await _cognitoClient.RespondToAuthChallengeAsync(challengeRequest, ct);

        var getUserRequest = new GetUserRequest
        {
            AccessToken = challengeResponse.AuthenticationResult.AccessToken
        };

        var cognitoUser = await _cognitoClient.GetUserAsync(getUserRequest, ct);
        var email = cognitoUser.UserAttributes.FirstOrDefault(a => a.Name == "email")?.Value ?? string.Empty;
        var userProfile = await _userServiceClient.GetUserProfileByEmailAsync(email, ct);

        _logger.LogInformation("MFA verification successful for user {Email}", email);

        return BuildLoginResponse(challengeResponse.AuthenticationResult, userProfile);
    }

    /// <inheritdoc />
    public async Task ForgotPasswordAsync(string email, CancellationToken ct)
    {
        _logger.LogInformation("Initiating forgot password flow for {Email}", email);

        var request = new ForgotPasswordRequest
        {
            ClientId = _cognitoSettings.ClientId,
            Username = email
        };

        try
        {
            await _cognitoClient.ForgotPasswordAsync(request, ct);
            _logger.LogInformation("Password reset code sent for {Email}", email);
        }
        catch (UserNotFoundException)
        {
            // Silently swallow to prevent email enumeration attacks
            _logger.LogWarning("Forgot password requested for non-existent user {Email}", email);
        }
    }

    /// <inheritdoc />
    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct)
    {
        _logger.LogInformation("Completing password reset");

        // The token format is expected to be "email:confirmationCode"
        var parts = token.Split(':', 2);
        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid reset token format.");
        }

        var email = parts[0];
        var confirmationCode = parts[1];

        var request = new ConfirmForgotPasswordRequest
        {
            ClientId = _cognitoSettings.ClientId,
            Username = email,
            ConfirmationCode = confirmationCode,
            Password = newPassword
        };

        await _cognitoClient.ConfirmForgotPasswordAsync(request, ct);

        _logger.LogInformation("Password reset completed for {Email}", email);
    }

    private static LoginResponse BuildLoginResponse(
        AuthenticationResultType authResult,
        UserProfileDto? userProfile,
        string? overrideRefreshToken = null)
    {
        return new LoginResponse
        {
            RequiresMfa = false,
            User = new UserInfo
            {
                Id = userProfile?.Id.ToString() ?? string.Empty,
                Email = userProfile?.Email ?? string.Empty,
                Name = userProfile?.FullName ?? string.Empty,
                Role = userProfile?.Role.ToString() ?? string.Empty,
                IsActive = userProfile?.IsActive ?? true,
                MfaEnabled = false,
                LastLoginAt = userProfile?.LastLoginAt
            },
            Tokens = new TokenInfo
            {
                AccessToken = authResult.AccessToken,
                RefreshToken = overrideRefreshToken ?? authResult.RefreshToken ?? string.Empty,
                ExpiresIn = authResult.ExpiresIn
            }
        };
    }
}
