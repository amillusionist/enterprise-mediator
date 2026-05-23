using FluentValidation;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Milestones.Commands.AddMilestone;

public class AddMilestoneValidator : AbstractValidator<AddMilestoneCommand>
{
    public AddMilestoneValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}
