namespace EnterpriseMediator.Core.SharedKernel.Common;

/// <summary>
/// Base class for all domain entities with a strongly-typed identifier.
/// Provides identity-based equality and domain event support.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier (e.g., ProjectId, VendorId).</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    private readonly List<Abstractions.IDomainEvent> _domainEvents = new();

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// The unique identifier for this entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Domain events raised by this entity that have not yet been dispatched.
    /// </summary>
    public IReadOnlyCollection<Abstractions.IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event to be dispatched after persistence.
    /// </summary>
    public void AddDomainEvent(Abstractions.IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event.
    /// </summary>
    public void RemoveDomainEvent(Abstractions.IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all pending domain events. Called by the infrastructure after dispatch.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Equals(entity);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
