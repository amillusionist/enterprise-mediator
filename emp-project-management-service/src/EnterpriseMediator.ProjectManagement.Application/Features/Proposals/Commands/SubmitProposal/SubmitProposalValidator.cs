using FluentValidation;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Proposals.Commands.SubmitProposal;

public class SubmitProposalValidator : AbstractValidator<SubmitProposalCommand>
{
    public SubmitProposalValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.ProposedCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("Currency must be a 3-letter ISO code.");
        RuleFor(x => x.Timeline).NotEmpty().MaximumLength(500);
        RuleFor(x => x.KeyPersonnel).NotEmpty().MaximumLength(2000);
    }
}
