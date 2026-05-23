using System.ComponentModel.DataAnnotations;

namespace EnterpriseMediator.Contracts.DTOs.Auth;

/// <summary>
/// Authentication login request payload.
/// </summary>
public record LoginRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }
    [Required]
    public required string Password { get; init; }
}
