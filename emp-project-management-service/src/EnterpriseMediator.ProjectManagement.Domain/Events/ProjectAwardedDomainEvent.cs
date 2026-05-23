using MediatR;

namespace EnterpriseMediator.ProjectManagement.Domain.Events;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

public sealed record ProjectCreatedDomainEvent(
    Guid ProjectId,
    Guid ClientId,
    string ProjectName) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record SowUploadedDomainEvent(
    Guid ProjectId,
    Guid SowDocumentId,
    string StorageKey) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record ProjectStatusChangedDomainEvent(
    Guid ProjectId,
    string OldStatus,
    string NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record BriefApprovedDomainEvent(
    Guid ProjectId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record ProjectAwardedDomainEvent(
    Guid ProjectId,
    Guid VendorId,
    Guid ProposalId,
    decimal AwardedAmount,
    string Currency) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record MilestoneApprovedDomainEvent(
    Guid ProjectId,
    Guid MilestoneId,
    decimal PayoutAmount) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
