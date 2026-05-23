namespace EnterpriseMediator.Contracts.DTOs.Vendors;

/// <summary>
/// Request payload for updating an existing vendor profile.
/// </summary>
public record UpdateVendorRequest
{
    public string? CompanyName { get; init; }
    public string? Address { get; init; }
    public string? PrimaryContactName { get; init; }
    public string? PrimaryContactEmail { get; init; }
    public string? PrimaryContactPhone { get; init; }
    public string[]? Skills { get; init; }
}
