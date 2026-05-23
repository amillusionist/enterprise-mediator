using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using Emp.ApiGateway.Infrastructure.Configuration;
using EnterpriseMediator.Contracts.DTOs.Auth;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Emp.ApiGateway.Infrastructure.Services;

/// <summary>
/// DEV ONLY AUTH BYPASS — intercepts login for configured local users; delegates all other flows to Cognito.
/// </summary>
public sealed class LocalDevAuthServiceDecorator : ICognitoAuthService
{
    private readonly CognitoAuthService _cognitoAuthService;
    private readonly LocalDevAuthTokenService _tokenService;
    private readonly LocalDevAuthOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<LocalDevAuthServiceDecorator> _logger;

    public LocalDevAuthServiceDecorator(
        CognitoAuthService cognitoAuthService,
        LocalDevAuthTokenService tokenService,
        IOptions<LocalDevAuthOptions> options,
        IHostEnvironment environment,
        ILogger<LocalDevAuthServiceDecorator> logger)
    {
        _cognitoAuthService = cognitoAuthService;
        _tokenService = tokenService;
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public Task<LoginResponse> LoginAsync(string email, string password, CancellationToken ct)
    {
        if (IsLocalDevActive)
        {
            if (_options.Users.Count == 0)
            {
                _logger.LogWarning("DEV ONLY AUTH BYPASS is enabled but no LocalDevAuth:Users are configured.");
            }

            if (TryGetLocalUser(email, password, out var user))
            {
                _logger.LogWarning("DEV ONLY AUTH BYPASS: local login accepted for {Email}", email);
                return Task.FromResult(BuildLocalLoginResponse(user!));
            }

            // DEV ONLY: never call Cognito for @local.dev accounts — avoids AWS errors in Docker
            if (email.EndsWith("@local.dev", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid local development credentials.");
            }
        }

        return _cognitoAuthService.LoginAsync(email, password, ct);
    }

    public Task LogoutAsync(string accessToken, CancellationToken ct)
    {
        if (_tokenService.IsLocalDevAccessToken(accessToken))
        {
            _logger.LogInformation("DEV ONLY AUTH BYPASS: local logout (no remote session).");
            return Task.CompletedTask;
        }

        return _cognitoAuthService.LogoutAsync(accessToken, ct);
    }

    public Task<LoginResponse> RegisterAsync(string inviteToken, string name, string password, CancellationToken ct) =>
        _cognitoAuthService.RegisterAsync(inviteToken, name, password, ct);

    public Task<LoginResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        if (_tokenService.TryValidateRefreshToken(refreshToken, out var user) && user is not null)
        {
            _logger.LogInformation("DEV ONLY AUTH BYPASS: refreshed local token for {Email}", user.Email);
            return Task.FromResult(BuildLocalLoginResponse(user));
        }

        return _cognitoAuthService.RefreshTokenAsync(refreshToken, ct);
    }

    public Task<LoginResponse> VerifyMfaAsync(string sessionId, string code, CancellationToken ct) =>
        _cognitoAuthService.VerifyMfaAsync(sessionId, code, ct);

    public Task ForgotPasswordAsync(string email, CancellationToken ct) =>
        _cognitoAuthService.ForgotPasswordAsync(email, ct);

    public Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct) =>
        _cognitoAuthService.ResetPasswordAsync(token, newPassword, ct);

    private bool IsLocalDevActive =>
        _environment.IsDevelopment() && _options.Enabled && _options.SigningKey.Length >= 32;

    private bool TryGetLocalUser(string email, string password, out LocalDevAuthUserOptions? user)
    {
        user = null;
        if (!IsLocalDevActive)
        {
            return false;
        }

        user = _options.Users.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)
            && u.Password == password);

        return user is not null;
    }

    private LoginResponse BuildLocalLoginResponse(LocalDevAuthUserOptions user)
    {
        var userId = new Guid(System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes(user.Email.ToLowerInvariant()))).ToString();

        return new LoginResponse
        {
            RequiresMfa = false,
            User = new UserInfo
            {
                Id = userId,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role,
                IsActive = true,
                MfaEnabled = false,
            },
            Tokens = new TokenInfo
            {
                AccessToken = _tokenService.CreateAccessToken(user),
                RefreshToken = _tokenService.CreateRefreshToken(user),
                ExpiresIn = _options.AccessTokenExpirationMinutes * 60,
            },
        };
    }
}
