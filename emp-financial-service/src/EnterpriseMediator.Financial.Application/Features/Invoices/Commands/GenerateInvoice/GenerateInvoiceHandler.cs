using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Interfaces;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Application.Features.Invoices.Commands.GenerateInvoice;

public class GenerateInvoiceHandler : IRequestHandler<GenerateInvoiceCommand, Result<Guid>>
{
    private readonly IFinancialRepository _repository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<GenerateInvoiceHandler> _logger;

    public GenerateInvoiceHandler(
        IFinancialRepository repository,
        IPaymentGateway paymentGateway,
        ILogger<GenerateInvoiceHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Guid>> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Initiating invoice generation for Project {ProjectId} Client {ClientId}",
                request.ProjectId, request.ClientId);

            // Idempotency: ensure no duplicate invoice per project
            var existingInvoice = await _repository.GetInvoiceByProjectIdAsync(request.ProjectId, cancellationToken);
            if (existingInvoice != null)
            {
                _logger.LogWarning("Duplicate invoice attempt for Project {ProjectId}. Existing Invoice {InvoiceId}",
                    request.ProjectId, existingInvoice.Id);
                return Result<Guid>.Failure($"An invoice already exists for Project {request.ProjectId}");
            }

            // Create domain entity via factory method
            var money = new Money(request.Amount, Currency.FromCode(request.CurrencyCode));
            var invoice = Invoice.Create(request.ProjectId, request.ClientId, money);

            // Generate payment link via gateway
            _logger.LogInformation("Creating payment link for Invoice {InvoiceId}", invoice.Id);
            var gatewayResult = await _paymentGateway.CreatePaymentLinkAsync(invoice, cancellationToken);

            if (string.IsNullOrEmpty(gatewayResult.PaymentId))
            {
                _logger.LogError("Payment gateway returned invalid result for Invoice {InvoiceId}", invoice.Id);
                return Result<Guid>.Failure("Failed to generate payment link with the payment provider.");
            }

            // Associate payment intent with invoice and transition to Sent
            invoice.SetPaymentIntent(gatewayResult.PaymentId);

            // Persist
            await _repository.AddInvoiceAsync(invoice, cancellationToken);
            await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Invoice {InvoiceId} generated successfully for Project {ProjectId}",
                invoice.Id, request.ProjectId);

            return Result<Guid>.Success(invoice.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating invoice for Project {ProjectId}", request.ProjectId);
            return Result<Guid>.Failure("An unexpected error occurred while generating the invoice.");
        }
    }
}
