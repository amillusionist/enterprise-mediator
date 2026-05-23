using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.DTOs.Financials;

/// <summary>
/// Client invoice details.
/// </summary>
public record InvoiceDto
{
    public required Guid Id { get; init; }
    public required Guid ProjectId { get; init; }
    public required Guid ClientId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required InvoiceStatus Status { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public string? StripeSessionId { get; init; }
    public string? InvoiceNumber { get; init; }
    public string? ClientSecret { get; init; }
    public string? PaymentLink { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
