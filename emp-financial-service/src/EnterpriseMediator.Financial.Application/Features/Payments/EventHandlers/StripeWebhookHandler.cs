using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;
using EnterpriseMediator.Financial.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Application.Features.Payments.EventHandlers;

public record ProcessStripePaymentCommand(
    string PaymentIntentId,
    long AmountReceived,
    string CurrencyCode,
    string ExternalReferenceId) : IRequest<Result>;

public class StripeWebhookHandler : IRequestHandler<ProcessStripePaymentCommand, Result>
{
    private readonly IFinancialRepository _repository;
    private readonly ILogger<StripeWebhookHandler> _logger;

    public StripeWebhookHandler(
        IFinancialRepository repository,
        ILogger<StripeWebhookHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(ProcessStripePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing Stripe payment webhook for Intent {PaymentIntentId}",
                request.PaymentIntentId);

            // Idempotency: check if transaction already recorded
            var alreadyProcessed = await _repository.ExistsTransactionWithExternalIdAsync(
                request.ExternalReferenceId, cancellationToken);
            if (alreadyProcessed)
            {
                _logger.LogInformation("Payment {ExternalReferenceId} already processed. Idempotent return.",
                    request.ExternalReferenceId);
                return Result.Success();
            }

            // Retrieve the invoice by PaymentIntentId
            var invoice = await _repository.GetInvoiceByPaymentIntentIdAsync(
                request.PaymentIntentId, cancellationToken);

            if (invoice is null)
            {
                _logger.LogCritical("Received payment for unknown Intent {PaymentIntentId}. Manual intervention required.",
                    request.PaymentIntentId);
                return Result.Failure($"Invoice not found for PaymentIntent {request.PaymentIntentId}");
            }

            // Already paid - idempotent
            if (invoice.Status == InvoiceStatus.Paid)
            {
                _logger.LogInformation("Invoice {InvoiceId} already marked as PAID. Idempotent success.", invoice.Id);
                return Result.Success();
            }

            // Validate amount (Stripe sends in smallest unit - cents)
            var receivedDecimal = request.AmountReceived / 100m;
            if (invoice.TotalAmount.Amount != receivedDecimal)
            {
                _logger.LogWarning("Payment mismatch for Invoice {InvoiceId}. Expected {Expected}, Received {Received}",
                    invoice.Id, invoice.TotalAmount.Amount, receivedDecimal);
            }

            // Update domain entity
            invoice.MarkAsPaid(request.ExternalReferenceId, DateTime.UtcNow);

            // Create immutable ledger transaction using factory method
            var transaction = Transaction.RecordPayment(invoice, request.ExternalReferenceId);

            await _repository.AddTransactionAsync(transaction, cancellationToken);
            await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reconciled payment for Invoice {InvoiceId}. Transaction {TransactionId} created.",
                invoice.Id, transaction.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook for Intent {PaymentIntentId}",
                request.PaymentIntentId);
            return Result.Failure(ex.Message);
        }
    }
}
