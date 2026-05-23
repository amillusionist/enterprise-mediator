using EnterpriseMediator.Financial.Application.Features.Invoices.Commands.GenerateInvoice;
using EnterpriseMediator.Financial.Application.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Infrastructure.Messaging.Consumers;

public class ProjectAwardedConsumer : IConsumer<ProjectAwardedIntegrationEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<ProjectAwardedConsumer> _logger;

    public ProjectAwardedConsumer(ISender sender, ILogger<ProjectAwardedConsumer> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<ProjectAwardedIntegrationEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received ProjectAwardedIntegrationEvent for Project {ProjectId}, Vendor {VendorId}",
            message.ProjectId, message.VendorId);

        var command = new GenerateInvoiceCommand(
            message.ProjectId,
            Guid.Empty, // ClientId would be resolved from project; for now placeholder
            message.ProposedCost,
            message.Currency);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to generate invoice for Project {ProjectId}: {Error}",
                message.ProjectId, result.Error);
        }
        else
        {
            _logger.LogInformation("Invoice {InvoiceId} generated for Project {ProjectId}",
                result.Value, message.ProjectId);
        }
    }
}
