using EnterpriseMediator.UserManagement.Domain.Events;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Infrastructure.Messaging;

/// <summary>
/// Handles the in-process VendorProfileUpdated domain event and publishes
/// the corresponding VendorProfileUpdatedIntegrationEvent to the message bus.
/// </summary>
public class VendorProfileUpdatedEventHandler : INotificationHandler<VendorProfileUpdated>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<VendorProfileUpdatedEventHandler> _logger;

    public VendorProfileUpdatedEventHandler(
        IPublishEndpoint publishEndpoint,
        IVendorRepository vendorRepository,
        ILogger<VendorProfileUpdatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(VendorProfileUpdated notification, CancellationToken cancellationToken)
    {
        string[] skills = Array.Empty<string>();

        if (notification.SkillsUpdated)
        {
            var vendor = await _vendorRepository.GetByIdAsync(notification.VendorId, cancellationToken);
            if (vendor is not null)
            {
                skills = vendor.Skills.Select(s => s.Name).ToArray();
            }
        }

        var integrationEvent = new VendorProfileUpdatedIntegrationEvent
        {
            VendorId = notification.VendorId,
            Skills = skills,
            SkillsUpdated = notification.SkillsUpdated,
            UpdatedAt = notification.UpdatedAt
        };

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        _logger.LogInformation("Published VendorProfileUpdatedIntegrationEvent for vendor {VendorId}", notification.VendorId);
    }
}
