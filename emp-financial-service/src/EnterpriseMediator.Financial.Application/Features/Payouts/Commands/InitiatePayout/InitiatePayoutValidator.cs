using FluentValidation;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Commands.InitiatePayout;

public class InitiatePayoutValidator : AbstractValidator<InitiatePayoutCommand>
{
    public InitiatePayoutValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty()
            .WithMessage("Vendor ID is required.");

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Payout amount must be greater than zero.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency code must be exactly 3 characters (ISO 4217).");
    }
}
