using EnterpriseMediator.UserManagement.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Infrastructure.Messaging;

/// <summary>
/// Handles the in-process VendorCreatedEvent domain event and publishes
/// the corresponding VendorCreatedIntegrationEvent to the message bus.
/// </summary>
public class VendorCreatedEventHandler : INotificationHandler<VendorCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<VendorCreatedEventHandler> _logger;

    public VendorCreatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<VendorCreatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(VendorCreatedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new VendorCreatedIntegrationEvent
        {
            VendorId = notification.VendorId,
            CompanyName = notification.CompanyName,
            PrimaryContactEmail = notification.PrimaryContactEmail,
            CreatedAt = notification.CreatedAt
        };

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        _logger.LogInformation("Published VendorCreatedIntegrationEvent for vendor {VendorId}", notification.VendorId);
    }
}
