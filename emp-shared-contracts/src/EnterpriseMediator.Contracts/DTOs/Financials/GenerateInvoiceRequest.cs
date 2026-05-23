using System.ComponentModel.DataAnnotations;

namespace EnterpriseMediator.Contracts.DTOs.Financials;

/// <summary>
/// Request to generate a client invoice for a project.
/// </summary>
public record GenerateInvoiceRequest
{
    [Required]
    public required Guid ProjectId { get; init; }

    [Range(0.01, double.MaxValue)]
    public required decimal Amount { get; init; }

    [Length(3, 3)]
    public required string Currency { get; init; }

    public DateTimeOffset? DueDate { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }
}
