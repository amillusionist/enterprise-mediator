namespace EnterpriseMediator.Contracts.DTOs.Vendors;

/// <summary>
/// Extended vendor profile with payment details and performance metrics.
/// </summary>
public record VendorDetailDto : VendorDto
{
    public string? Address { get; init; }
    public PaymentDetailsDto? PaymentDetails { get; init; }
    public int ProjectsAwarded { get; init; }
    public double? AverageRating { get; init; }
    public double? OnTimeCompletionRate { get; init; }
}

/// <summary>
/// Vendor bank account details for payout processing.
/// </summary>
public record PaymentDetailsDto
{
    public string? BankName { get; init; }
    public string? AccountNumber { get; init; }
    public string? SwiftCode { get; init; }
}
