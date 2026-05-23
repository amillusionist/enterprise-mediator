using FluentValidation;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty().WithMessage("Client ID is required.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Project name is required and must be under 200 characters.");
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
