using EnterpriseMediator.ProjectManagement.Application.Common;
using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Commands.SubmitProposal;

public class SubmitProposalCommandHandler : IRequestHandler<SubmitProposalCommand, Result<Guid>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<SubmitProposalCommandHandler> _logger;

    public SubmitProposalCommandHandler(IProjectRepository projectRepository, ILogger<SubmitProposalCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(SubmitProposalCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Submitting proposal for project {ProjectId} from vendor {VendorId}", request.ProjectId, request.VendorId);

        var project = await _projectRepository.GetByIdWithProposalsAsync(request.ProjectId, cancellationToken);
        if (project == null)
            return Result<Guid>.Failure($"Project {request.ProjectId} not found.");

        try
        {
            var proposal = new Proposal(
                request.ProjectId,
                request.VendorId,
                request.ProposedCost,
                request.Currency,
                request.Timeline,
                request.KeyPersonnel,
                request.CoverLetter,
                request.ProposalDocumentUrl);

            project.AddProposal(proposal);
            _projectRepository.Update(project);
            await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Proposal {ProposalId} submitted for project {ProjectId}", proposal.Id, request.ProjectId);
            return Result<Guid>.Success(proposal.Id);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to submit proposal: {Message}", ex.Message);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
