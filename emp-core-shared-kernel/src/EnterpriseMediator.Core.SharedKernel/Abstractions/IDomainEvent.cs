using MediatR;

namespace EnterpriseMediator.Core.SharedKernel.Abstractions;

/// <summary>
/// Marker interface for domain events. Domain events are published via MediatR
/// and dispatched after aggregate state changes are persisted.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// UTC timestamp of when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
