using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.DTOs.Financials;

/// <summary>
/// Immutable financial transaction record from the ledger.
/// </summary>
public record TransactionDto
{
    public required Guid Id { get; init; }
    public required Guid ProjectId { get; init; }
    public required TransactionType Type { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public string? ReferenceId { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
    public Guid? VendorId { get; init; }
    public Guid? ClientId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
