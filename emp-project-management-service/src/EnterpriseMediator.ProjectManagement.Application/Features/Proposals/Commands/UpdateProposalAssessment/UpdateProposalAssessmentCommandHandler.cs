using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Commands.UpdateProposalAssessment;

public class UpdateProposalAssessmentCommandHandler : IRequestHandler<UpdateProposalAssessmentCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<UpdateProposalAssessmentCommandHandler> _logger;

    public UpdateProposalAssessmentCommandHandler(IProjectRepository projectRepository, ILogger<UpdateProposalAssessmentCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task Handle(UpdateProposalAssessmentCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdWithProposalsAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        var proposal = project.Proposals.FirstOrDefault(p => p.Id == request.ProposalId)
            ?? throw new KeyNotFoundException($"Proposal {request.ProposalId} not found.");

        proposal.UpdateAssessment(request.Score, request.Flag);
        _projectRepository.Update(project);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assessment updated for proposal {ProposalId}", request.ProposalId);
    }
}
