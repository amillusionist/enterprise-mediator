using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.DTOs.Vendors;

/// <summary>
/// Vendor profile response.
/// </summary>
public record VendorDto
{
    public required Guid Id { get; init; }
    public required string CompanyName { get; init; }
    public string? PrimaryContactEmail { get; init; }
    public string? PrimaryContactPhone { get; init; }
    public required VendorStatus Status { get; init; }
    public required string[] Skills { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? Location => City != null && Country != null ? $"{City}, {Country}" : Country ?? City;
    public required DateTimeOffset CreatedAt { get; init; }
}
