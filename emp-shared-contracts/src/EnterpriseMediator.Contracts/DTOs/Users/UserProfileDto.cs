using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.DTOs.Users;

/// <summary>
/// User profile response from the user management service.
/// </summary>
public record UserProfileDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? FullName { get; init; }
    public required UserRole Role { get; init; }
    public required bool IsActive { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
}
