namespace EnterpriseMediator.Financial.Application.DTOs;

public record InvoiceDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid ClientId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? StripePaymentIntentId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PaidAt { get; init; }
}
