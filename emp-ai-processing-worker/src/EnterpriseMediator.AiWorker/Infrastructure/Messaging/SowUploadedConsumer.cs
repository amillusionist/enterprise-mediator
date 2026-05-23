using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using EnterpriseMediator.AiWorker.Features.SowProcessing;
using EnterpriseMediator.Contracts.Events.Projects;

namespace EnterpriseMediator.AiWorker.Infrastructure.Messaging;

/// <summary>
/// Consumes SowUploadedEvent messages from the message bus and orchestrates
/// the processing workflow via the application layer.
/// </summary>
public class SowUploadedConsumer : IConsumer<SowUploadedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<SowUploadedConsumer> _logger;

    public SowUploadedConsumer(
        IMediator mediator,
        ILogger<SowUploadedConsumer> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the consumption of the SowUploadedEvent.
    /// Acts as an adapter between the Message Bus and the Application Logic (MediatR).
    /// </summary>
    public async Task Consume(ConsumeContext<SowUploadedEvent> context)
    {
        var sowId = context.Message.SowId;
        var projectId = context.Message.ProjectId;
        var fileKey = context.Message.S3ObjectKey;
        var correlationId = context.Message.CorrelationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["SowId"] = sowId,
            ["ProjectId"] = projectId,
            ["CorrelationId"] = correlationId,
            ["MessageId"] = context.MessageId ?? Guid.Empty
        }))
        {
            try
            {
                _logger.LogInformation(
                    "Received SowUploadedEvent for SOW {SowId} in Project {ProjectId}. Processing starting.",
                    sowId,
                    projectId);

                if (string.IsNullOrWhiteSpace(fileKey))
                {
                    _logger.LogError("SowUploadedEvent received with empty S3ObjectKey for SOW {SowId}. Message cannot be processed.", sowId);
                    return;
                }

                var command = new ProcessSowCommand(
                    SowId: sowId,
                    FileKey: fileKey
                );

                var result = await _mediator.Send(command, context.CancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully processed SOW {SowId}.", sowId);
                }
                else
                {
                    _logger.LogWarning(
                        "SOW processing completed with failure status for {SowId}. Error: {ErrorMessage}",
                        sowId,
                        result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception consuming SowUploadedEvent for SOW {SowId}.", sowId);
                throw;
            }
        }
    }
}
