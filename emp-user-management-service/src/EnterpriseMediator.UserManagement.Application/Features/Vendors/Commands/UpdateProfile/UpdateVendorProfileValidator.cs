using FluentValidation;

namespace EnterpriseMediator.UserManagement.Application.Features.Vendors.Commands.UpdateProfile;

public class UpdateVendorProfileValidator : AbstractValidator<UpdateVendorProfileCommand>
{
    public UpdateVendorProfileValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(200)
            .When(x => x.CompanyName is not null);

        RuleFor(x => x.AddressLine1)
            .MaximumLength(200)
            .When(x => x.AddressLine1 is not null);

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => x.City is not null);

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => x.Country is not null);

        RuleFor(x => x.PaymentProvider)
            .MaximumLength(150)
            .When(x => x.PaymentProvider is not null);

        RuleFor(x => x.PaymentAccountNumber)
            .MaximumLength(100)
            .When(x => x.PaymentAccountNumber is not null);

        RuleForEach(x => x.Skills)
            .NotEmpty().WithMessage("Skill tag cannot be empty.")
            .MaximumLength(100)
            .When(x => x.Skills is not null);
    }
}
