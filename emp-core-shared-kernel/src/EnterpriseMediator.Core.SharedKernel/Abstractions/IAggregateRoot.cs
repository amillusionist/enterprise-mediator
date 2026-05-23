namespace EnterpriseMediator.Core.SharedKernel.Abstractions;

/// <summary>
/// Marker interface identifying an entity as an aggregate root.
/// Only aggregate roots should have dedicated repositories.
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Gets the domain events raised by this aggregate that have not yet been dispatched.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all pending domain events. Called after events have been dispatched.
    /// </summary>
    void ClearDomainEvents();
}
