using EnterpriseMediator.Contracts.Common;

namespace EnterpriseMediator.Contracts.Events.Financials;

/// <summary>
/// Published when a client payment is received via Stripe.
/// Triggers invoice status update and potentially vendor payout scheduling.
/// </summary>
public record PaymentReceivedEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public required Guid InvoiceId { get; init; }
    public required Guid ProjectId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string StripePaymentIntentId { get; init; }
}
