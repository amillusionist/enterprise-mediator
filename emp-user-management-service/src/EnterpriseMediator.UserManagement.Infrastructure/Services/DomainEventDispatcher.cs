using EnterpriseMediator.UserManagement.Domain.Aggregates.Client;
using EnterpriseMediator.UserManagement.Domain.Aggregates.User;
using EnterpriseMediator.UserManagement.Domain.Aggregates.Vendor;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Infrastructure.Services;

/// <summary>
/// Dispatches domain events from tracked entities after SaveChanges.
/// Integrates with UserDbContext to collect events from User, Vendor, and Client aggregates.
/// </summary>
public class DomainEventDispatcher
{
    private readonly IPublisher _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IPublisher mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DispatchEventsAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        var domainEvents = new List<INotification>();

        var userEntities = dbContext.ChangeTracker
            .Entries<User>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in userEntities)
        {
            domainEvents.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        var vendorEntities = dbContext.ChangeTracker
            .Entries<Vendor>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in vendorEntities)
        {
            domainEvents.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        var clientEntities = dbContext.ChangeTracker
            .Entries<Client>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in clientEntities)
        {
            domainEvents.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        if (domainEvents.Count == 0)
            return;

        _logger.LogDebug("Dispatching {Count} domain events", domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogDebug("Dispatched domain event: {EventName}", domainEvent.GetType().Name);
        }
    }
}
