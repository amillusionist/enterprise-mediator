using FluentValidation;

namespace EnterpriseMediator.UserManagement.Application.Features.Users.Commands.AnonymizeUser;

public class AnonymizeUserValidator : AbstractValidator<AnonymizeUserCommand>
{
    public AnonymizeUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Anonymization reason is required.")
            .MaximumLength(500);
    }
}
