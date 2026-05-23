using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Commands.UpdateProposalAssessment;

public record UpdateProposalAssessmentCommand(
    Guid ProjectId,
    Guid ProposalId,
    int? Score,
    string? Flag) : IRequest;
