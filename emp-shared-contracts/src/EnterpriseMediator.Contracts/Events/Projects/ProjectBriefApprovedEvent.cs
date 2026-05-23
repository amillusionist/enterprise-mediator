using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Projects;

namespace EnterpriseMediator.Contracts.Events.Projects;

/// <summary>
/// Published when an admin approves the AI-extracted project brief.
/// Triggers vendor matching workflow.
/// </summary>
public record ProjectBriefApprovedEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// The project whose brief was approved.
    /// </summary>
    public required Guid ProjectId { get; init; }

    /// <summary>
    /// The approved brief data, including skills and scope for embedding/matching.
    /// </summary>
    public required ProjectBriefDto Brief { get; init; }
}
