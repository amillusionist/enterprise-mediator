using EnterpriseMediator.Core.SharedKernel.Abstractions;

namespace EnterpriseMediator.Core.SharedKernel.Common;

/// <summary>
/// Convenience base record for domain events. Provides auto-generated EventId and OccurredOn.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
