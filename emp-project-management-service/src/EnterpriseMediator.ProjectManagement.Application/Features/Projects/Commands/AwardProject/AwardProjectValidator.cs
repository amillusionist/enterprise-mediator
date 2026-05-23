using FluentValidation;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.AwardProject;

public class AwardProjectValidator : AbstractValidator<AwardProjectCommand>
{
    public AwardProjectValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ProposalId).NotEmpty();
    }
}
