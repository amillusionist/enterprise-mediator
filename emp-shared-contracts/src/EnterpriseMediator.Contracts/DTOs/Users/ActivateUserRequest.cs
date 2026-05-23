using System.ComponentModel.DataAnnotations;

namespace EnterpriseMediator.Contracts.DTOs.Users;

/// <summary>
/// Request to activate a user from an invitation, setting their password.
/// </summary>
public record ActivateUserRequest
{
    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public required string Password { get; init; }

    [Required]
    [MaxLength(100)]
    public required string FullName { get; init; }
}
