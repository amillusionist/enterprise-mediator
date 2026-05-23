using EnterpriseMediator.UserManagement.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Infrastructure.Services;

public class AuditServiceAdapter : IAuditServiceAdapter
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuditServiceAdapter> _logger;

    public AuditServiceAdapter(
        IPublishEndpoint publishEndpoint,
        ILogger<AuditServiceAdapter> logger)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LogEventAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (entry is null)
        {
            _logger.LogWarning("Attempted to log a null audit entry. Operation skipped.");
            return;
        }

        try
        {
            await _publishEndpoint.Publish(entry, cancellationToken);

            _logger.LogDebug("Audit event {Action} for entity {EntityType}/{EntityId} published to bus",
                entry.Action,
                entry.EntityType,
                entry.EntityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish audit event {Action} for entity {EntityType}/{EntityId}",
                entry.Action,
                entry.EntityType,
                entry.EntityId);
        }
    }
}
