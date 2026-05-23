namespace EnterpriseMediator.Contracts.DTOs.Common;

/// <summary>
/// Tax configuration settings for financial operations.
/// </summary>
public record TaxSettingsDto
{
    public decimal DefaultTaxRate { get; init; }
    public string? TaxIdNumber { get; init; }
    public string? TaxRegion { get; init; }
    public bool ApplyTaxToInvoices { get; init; }
    public bool ApplyTaxToPayouts { get; init; }
}
