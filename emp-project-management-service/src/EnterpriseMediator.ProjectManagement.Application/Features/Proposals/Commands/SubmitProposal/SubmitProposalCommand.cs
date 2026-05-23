using EnterpriseMediator.ProjectManagement.Application.Common;
using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Commands.SubmitProposal;

public record SubmitProposalCommand(
    Guid ProjectId,
    Guid VendorId,
    decimal ProposedCost,
    string Currency,
    string Timeline,
    string KeyPersonnel,
    string CoverLetter,
    string? ProposalDocumentUrl) : IRequest<Result<Guid>>;
