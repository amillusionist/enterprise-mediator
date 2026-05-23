using EnterpriseMediator.UserManagement.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Infrastructure.Messaging;

/// <summary>
/// Handles the in-process UserRegisteredEvent domain event and publishes
/// the corresponding UserRegisteredIntegrationEvent to the message bus.
/// </summary>
public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(IPublishEndpoint publishEndpoint, ILogger<UserRegisteredEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserRegisteredIntegrationEvent
        {
            UserId = notification.UserId,
            Email = notification.Email,
            Role = notification.UserType,
            RegisteredAt = notification.RegisteredAt
        };

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        _logger.LogInformation("Published UserRegisteredIntegrationEvent for user {UserId}", notification.UserId);
    }
}
