using EnterpriseMediator.Core.SharedKernel.Abstractions;
using EnterpriseMediator.Core.SharedKernel.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Core.SharedKernel.Implementations;

/// <summary>
/// Dispatches domain events via MediatR after aggregate state changes are persisted.
/// Typically called from a DbContext override of SaveChangesAsync.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DispatchAndClearEvents(IEnumerable<object> entitiesWithEvents, CancellationToken cancellationToken = default)
    {
        var domainEntities = entitiesWithEvents
            .OfType<IAggregateRoot>()
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events before publishing to avoid re-entrancy issues
        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Dispatching domain event {EventType} ({EventId})",
                domainEvent.GetType().Name, domainEvent.EventId);

            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task Dispatch(object domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent is IDomainEvent typedEvent)
        {
            _logger.LogInformation("Dispatching domain event {EventType} ({EventId})",
                typedEvent.GetType().Name, typedEvent.EventId);

            await _mediator.Publish(typedEvent, cancellationToken);
        }
    }
}
