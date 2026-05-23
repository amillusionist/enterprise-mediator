using EnterpriseMediator.ProjectManagement.Application.Common;
using EnterpriseMediator.ProjectManagement.Application.Interfaces;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.AwardProject;

public record ProjectAwardedIntegrationEvent(
    Guid ProjectId,
    Guid VendorId,
    Guid ProposalId,
    decimal Amount,
    string Currency,
    DateTime OccurredOn);

public class AwardProjectCommandHandler : IRequestHandler<AwardProjectCommand, Result>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<AwardProjectCommandHandler> _logger;

    public AwardProjectCommandHandler(
        IProjectRepository projectRepository,
        IMessageBus messageBus,
        ILogger<AwardProjectCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task<Result> Handle(AwardProjectCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Awarding project {ProjectId} to proposal {ProposalId}", request.ProjectId, request.ProposalId);

        var project = await _projectRepository.GetByIdWithProposalsAsync(request.ProjectId, cancellationToken);
        if (project == null)
            return Result.Failure($"Project {request.ProjectId} not found.");

        try
        {
            project.AwardTo(request.ProposalId);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            _logger.LogWarning(ex, "Domain validation failed for project award: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }

        _projectRepository.Update(project);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var winningProposal = project.Proposals.First(p => p.Id == request.ProposalId);
        await _messageBus.PublishAsync(new ProjectAwardedIntegrationEvent(
            project.Id,
            winningProposal.VendorId,
            winningProposal.Id,
            winningProposal.ProposedCost,
            winningProposal.Currency,
            DateTime.UtcNow), cancellationToken);

        _logger.LogInformation("Project {ProjectId} awarded successfully", request.ProjectId);
        return Result.Success();
    }
}
