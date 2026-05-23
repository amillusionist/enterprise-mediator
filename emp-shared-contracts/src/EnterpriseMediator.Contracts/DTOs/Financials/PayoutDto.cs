using System.ComponentModel.DataAnnotations;
using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.DTOs.Financials;

/// <summary>
/// Vendor payout information.
/// </summary>
public record PayoutDto
{
    public required Guid Id { get; init; }

    [Required]
    public required Guid VendorId { get; init; }

    public required Guid ProjectId { get; init; }

    [Range(0.01, double.MaxValue)]
    public required decimal Amount { get; init; }

    [Length(3, 3)]
    public required string Currency { get; init; }

    public required PayoutStatus Status { get; init; }
    public string? WiseTransferId { get; init; }
    public string? VendorName { get; init; }
    public string? FailureReason { get; init; }
    public Guid? MilestoneId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
