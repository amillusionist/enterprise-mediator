using FluentValidation;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Commands.RejectPayout;

public class RejectPayoutValidator : AbstractValidator<RejectPayoutCommand>
{
    public RejectPayoutValidator()
    {
        RuleFor(x => x.PayoutId)
            .NotEmpty()
            .WithMessage("Payout ID is required.");

        RuleFor(x => x.RejectorId)
            .NotEmpty()
            .WithMessage("Rejector ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Rejection reason is required.")
            .MaximumLength(500)
            .WithMessage("Rejection reason must not exceed 500 characters.");
    }
}
