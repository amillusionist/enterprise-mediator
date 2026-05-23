using System.ComponentModel.DataAnnotations;

namespace EnterpriseMediator.Contracts.DTOs.Vendors;

/// <summary>
/// Request payload for registering a new vendor profile.
/// </summary>
public record CreateVendorRequest
{
    [Required]
    public required string CompanyName { get; init; }
    public string? Address { get; init; }
    [Required]
    public required string PrimaryContactName { get; init; }
    [Required, EmailAddress]
    public required string PrimaryContactEmail { get; init; }
    public string? PrimaryContactPhone { get; init; }
    public required string[] Skills { get; init; }
}
