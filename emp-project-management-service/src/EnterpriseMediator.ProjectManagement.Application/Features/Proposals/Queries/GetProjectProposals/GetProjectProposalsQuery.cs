using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Queries.GetProjectProposals;

public record GetProjectProposalsQuery(Guid ProjectId) : IRequest<IReadOnlyList<ProposalDto>>;

public record ProposalDto(
    Guid Id,
    Guid VendorId,
    decimal ProposedCost,
    string Currency,
    string Timeline,
    string KeyPersonnel,
    string Status,
    DateTimeOffset SubmittedAt,
    int? InternalScore,
    string? InternalFlag);
