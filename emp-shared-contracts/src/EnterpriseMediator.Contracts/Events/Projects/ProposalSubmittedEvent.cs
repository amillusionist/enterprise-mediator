using EnterpriseMediator.Contracts.Common;

namespace EnterpriseMediator.Contracts.Events.Projects;

/// <summary>
/// Published when a vendor submits a proposal for a project.
/// Triggers notification to the admin for review.
/// </summary>
public record ProposalSubmittedEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public required Guid ProposalId { get; init; }
    public required Guid ProjectId { get; init; }
    public required Guid VendorId { get; init; }
}
