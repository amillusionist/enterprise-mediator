using EnterpriseMediator.Core.SharedKernel.Abstractions;

namespace EnterpriseMediator.Core.SharedKernel.Common;

/// <summary>
/// Base class for aggregate roots — the transactional consistency boundary in DDD.
/// Only aggregate roots should be persisted directly via repositories.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : notnull
{
    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }
}
