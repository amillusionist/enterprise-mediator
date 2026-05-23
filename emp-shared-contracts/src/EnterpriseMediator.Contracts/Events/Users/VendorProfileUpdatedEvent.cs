using EnterpriseMediator.Contracts.Common;

namespace EnterpriseMediator.Contracts.Events.Users;

/// <summary>
/// Published when a vendor updates their profile, particularly skills.
/// Triggers re-embedding of vendor skill vectors.
/// </summary>
public record VendorProfileUpdatedEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public required Guid VendorId { get; init; }
    public required string[] Skills { get; init; }
    public required bool SkillsUpdated { get; init; }
}
