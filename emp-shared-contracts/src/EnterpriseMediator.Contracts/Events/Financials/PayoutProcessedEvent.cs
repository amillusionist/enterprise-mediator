using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.Events.Financials;

/// <summary>
/// Published when a vendor payout is processed via Wise.
/// </summary>
public record PayoutProcessedEvent : IIntegrationEvent
{
    public required Guid EventId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public required Guid PayoutId { get; init; }
    public required Guid VendorId { get; init; }
    public required Guid ProjectId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required PayoutStatus Status { get; init; }
    public string? WiseTransferId { get; init; }
}
