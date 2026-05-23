using FluentValidation;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.UpdateProjectBrief;

public class UpdateProjectBriefValidator : AbstractValidator<UpdateProjectBriefCommand>
{
    public UpdateProjectBriefValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ScopeSummary).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.RequiredSkills).NotEmpty().WithMessage("At least one required skill must be specified.");
        RuleFor(x => x.Timeline).NotEmpty();
    }
}
