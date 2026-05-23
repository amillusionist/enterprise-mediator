using EnterpriseMediator.Contracts.Common;

namespace EnterpriseMediator.Contracts.Events.Projects;

/// <summary>
/// Published when a client approves a project milestone via the secure approval link.
/// Triggers payout processing in the financial service.
/// </summary>
public record MilestoneApprovedEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public required Guid ProjectId { get; init; }
    public required Guid MilestoneId { get; init; }
    public required Guid VendorId { get; init; }
    public required decimal PayoutAmount { get; init; }
    public required string Currency { get; init; }
}
