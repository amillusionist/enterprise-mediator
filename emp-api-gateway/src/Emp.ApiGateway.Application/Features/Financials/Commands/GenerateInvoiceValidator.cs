using FluentValidation;

namespace Emp.ApiGateway.Application.Features.Financials.Commands;

public class GenerateInvoiceValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Invoice amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency code must be exactly 3 characters (ISO 4217).");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
