using FluentValidation;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.ChangeProjectStatus;

public class ChangeProjectStatusValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    private static readonly string[] ValidActions = { "activate", "complete", "hold", "resume", "cancel" };

    public ChangeProjectStatusValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty()
            .Must(a => ValidActions.Contains(a.ToLowerInvariant()))
            .WithMessage("Action must be one of: activate, complete, hold, resume, cancel.");
    }
}
