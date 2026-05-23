namespace EnterpriseMediator.Contracts.DTOs.Auth;

/// <summary>
/// Request payload for refreshing an expired access token.
/// </summary>
public record RefreshTokenRequest
{
    public string? RefreshToken { get; init; }
}
