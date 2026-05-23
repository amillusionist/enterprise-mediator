using System.ComponentModel.DataAnnotations;

namespace EnterpriseMediator.Contracts.DTOs.Auth;

/// <summary>
/// Request payload for completing user registration via invite token.
/// </summary>
public record RegisterRequest
{
    [Required]
    public required string InviteToken { get; init; }
    [Required]
    public required string Name { get; init; }
    [Required, MinLength(8)]
    public required string Password { get; init; }
}
