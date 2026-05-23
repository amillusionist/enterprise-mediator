using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Queries.GetProjectProposals;

public class GetProjectProposalsQueryHandler : IRequestHandler<GetProjectProposalsQuery, IReadOnlyList<ProposalDto>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectProposalsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<IReadOnlyList<ProposalDto>> Handle(GetProjectProposalsQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdWithProposalsAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} not found.");

        return project.Proposals.Select(p => new ProposalDto(
            p.Id,
            p.VendorId,
            p.ProposedCost,
            p.Currency,
            p.Timeline,
            p.KeyPersonnel,
            p.Status.ToString(),
            p.SubmittedAt,
            p.InternalScore,
            p.InternalFlag)).ToList();
    }
}
