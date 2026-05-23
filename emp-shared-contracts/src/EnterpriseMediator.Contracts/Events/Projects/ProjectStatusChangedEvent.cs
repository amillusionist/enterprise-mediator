using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.Events.Projects;

/// <summary>
/// Published when a project transitions between lifecycle states.
/// Consumed by audit, notification, and financial services.
/// </summary>
public record ProjectStatusChangedEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public required Guid ProjectId { get; init; }
    public required ProjectStatus OldStatus { get; init; }
    public required ProjectStatus NewStatus { get; init; }
    public required Guid ChangedBy { get; init; }
}
