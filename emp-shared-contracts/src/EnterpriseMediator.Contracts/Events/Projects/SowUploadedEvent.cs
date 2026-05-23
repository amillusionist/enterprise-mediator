using EnterpriseMediator.Contracts.Common;

namespace EnterpriseMediator.Contracts.Events.Projects;

/// <summary>
/// Published when an SOW document is uploaded and stored in S3.
/// Triggers the AI Worker to begin asynchronous SOW processing.
/// </summary>
public record SowUploadedEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// The SOW document entity identifier.
    /// </summary>
    public required Guid SowId { get; init; }

    /// <summary>
    /// The project this SOW belongs to.
    /// </summary>
    public required Guid ProjectId { get; init; }

    /// <summary>
    /// S3 object key where the SOW file is stored.
    /// </summary>
    public required string S3ObjectKey { get; init; }
}
