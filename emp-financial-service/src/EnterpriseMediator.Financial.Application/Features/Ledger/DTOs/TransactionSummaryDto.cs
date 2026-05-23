namespace EnterpriseMediator.Financial.Application.Features.Ledger.DTOs;

public record TransactionSummaryDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public Guid ProjectId { get; init; }
    public Guid? InvoiceId { get; init; }
    public Guid? PayoutId { get; init; }
    public string ExternalReferenceId { get; init; } = string.Empty;
    public string? Description { get; init; }
}
