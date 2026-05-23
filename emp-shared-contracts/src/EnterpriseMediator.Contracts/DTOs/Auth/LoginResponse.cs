namespace EnterpriseMediator.Contracts.DTOs.Auth;

/// <summary>
/// Authentication login response containing user info and tokens.
/// </summary>
public record LoginResponse
{
    public required UserInfo User { get; init; }
    public required TokenInfo Tokens { get; init; }
    public bool RequiresMfa { get; init; }
    public string? MfaSessionId { get; init; }
}

/// <summary>
/// Authenticated user profile information.
/// </summary>
public record UserInfo
{
    public required string Id { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
    public bool IsActive { get; init; }
    public bool MfaEnabled { get; init; }
    public string? AvatarUrl { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
}

/// <summary>
/// OAuth token information returned after successful authentication.
/// </summary>
public record TokenInfo
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
}
