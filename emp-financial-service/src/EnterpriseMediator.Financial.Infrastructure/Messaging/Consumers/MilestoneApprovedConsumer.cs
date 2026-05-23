using EnterpriseMediator.Financial.Application.Features.Payouts.Commands.InitiatePayout;
using EnterpriseMediator.Financial.Application.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Infrastructure.Messaging.Consumers;

public class MilestoneApprovedConsumer : IConsumer<MilestoneApprovedIntegrationEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<MilestoneApprovedConsumer> _logger;

    public MilestoneApprovedConsumer(ISender sender, ILogger<MilestoneApprovedConsumer> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<MilestoneApprovedIntegrationEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received MilestoneApprovedIntegrationEvent for Project {ProjectId}, Milestone {MilestoneId}",
            message.ProjectId, message.MilestoneId);

        var command = new InitiatePayoutCommand(
            message.VendorId,
            message.ProjectId,
            message.PayoutAmount,
            message.Currency);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to initiate payout for Project {ProjectId}: {Error}",
                message.ProjectId, result.Error);
        }
        else
        {
            _logger.LogInformation("Payout {PayoutId} initiated for Project {ProjectId}, Milestone {MilestoneId}",
                result.Value, message.ProjectId, message.MilestoneId);
        }
    }
}
