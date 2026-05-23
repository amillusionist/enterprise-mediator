using FluentValidation;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Commands.ApprovePayout;

public class ApprovePayoutValidator : AbstractValidator<ApprovePayoutCommand>
{
    public ApprovePayoutValidator()
    {
        RuleFor(x => x.PayoutId)
            .NotEmpty()
            .WithMessage("Payout ID is required.");

        RuleFor(x => x.ApproverId)
            .NotEmpty()
            .WithMessage("Approver ID is required.");
    }
}
