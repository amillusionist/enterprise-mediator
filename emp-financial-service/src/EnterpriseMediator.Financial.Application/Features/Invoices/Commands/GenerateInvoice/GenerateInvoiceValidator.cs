using FluentValidation;

namespace EnterpriseMediator.Financial.Application.Features.Invoices.Commands.GenerateInvoice;

public class GenerateInvoiceValidator : AbstractValidator<GenerateInvoiceCommand>
{
    public GenerateInvoiceValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Invoice amount must be greater than zero.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage("Currency code is required.")
            .Length(3)
            .WithMessage("Currency code must be exactly 3 characters (ISO 4217).");
    }
}
