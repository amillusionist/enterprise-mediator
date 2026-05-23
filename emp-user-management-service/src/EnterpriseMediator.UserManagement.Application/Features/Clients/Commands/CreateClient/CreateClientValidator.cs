using FluentValidation;

namespace EnterpriseMediator.UserManagement.Application.Features.Clients.Commands.CreateClient;

public class CreateClientValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        RuleFor(x => x.PrimaryContactEmail)
            .NotEmpty().WithMessage("Primary contact email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(255);

        RuleFor(x => x.PrimaryContactFirstName)
            .NotEmpty().WithMessage("Primary contact first name is required.")
            .MaximumLength(100);

        RuleFor(x => x.PrimaryContactLastName)
            .NotEmpty().WithMessage("Primary contact last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required.")
            .MaximumLength(200);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100);

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100);
    }
}
