using MediatR;

namespace EnterpriseMediator.UserManagement.Application.Features.Vendors.Queries.GetVendorDetails;

public sealed record GetVendorDetailsQuery(Guid VendorId) : IRequest<VendorDetailsDto>;

public record VendorDetailsDto
{
    public Guid Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string PrimaryContactEmail { get; init; } = string.Empty;
    public string VettingStatus { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public VendorAddressDto? Address { get; init; }
    public VendorPaymentInfoDto? PaymentDetails { get; init; }
    public List<string> Skills { get; init; } = new();
}

public record VendorAddressDto
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public record VendorPaymentInfoDto
{
    public string ProviderName { get; init; } = string.Empty;
    public string MaskedAccountIdentifier { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
}
