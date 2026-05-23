using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.Events.Users;

/// <summary>
/// Published when a new user completes registration.
/// </summary>
public record UserRegisteredEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required UserRole Role { get; init; }
}
