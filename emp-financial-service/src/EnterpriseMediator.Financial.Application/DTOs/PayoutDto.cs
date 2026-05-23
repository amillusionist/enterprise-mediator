namespace EnterpriseMediator.Financial.Application.DTOs;

public record PayoutDto
{
    public Guid Id { get; init; }
    public Guid VendorId { get; init; }
    public Guid ProjectId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? WiseTransferId { get; init; }
    public Guid? ApproverId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? FailureReason { get; init; }
}
