using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.DTOs.Users;

/// <summary>
/// User invitation details returned when validating an invitation token.
/// </summary>
public record UserInvitationDto
{
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required bool IsValid { get; init; }
}
