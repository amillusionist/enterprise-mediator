using EnterpriseMediator.Contracts.Common;

namespace EnterpriseMediator.Contracts.Events.Projects;

/// <summary>
/// Published when a project is awarded to a vendor.
/// Triggers financial service to set up invoicing and payout schedules.
/// </summary>
public record ProjectAwardedEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public required Guid ProjectId { get; init; }
    public required Guid VendorId { get; init; }
    public required Guid ProposalId { get; init; }
    public required decimal AwardedAmount { get; init; }
    public required string Currency { get; init; }
}
