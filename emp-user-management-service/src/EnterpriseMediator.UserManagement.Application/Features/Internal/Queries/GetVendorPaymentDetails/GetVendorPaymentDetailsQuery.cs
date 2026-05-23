using MediatR;

namespace EnterpriseMediator.UserManagement.Application.Features.Internal.Queries.GetVendorPaymentDetails;

public sealed record GetVendorPaymentDetailsQuery(Guid VendorId) : IRequest<PaymentDetailsDto>;

public record PaymentDetailsDto
{
    public string ProviderName { get; init; } = string.Empty;
    public string AccountIdentifier { get; init; } = string.Empty;
    public string? RoutingIdentifier { get; init; }
    public string Currency { get; init; } = string.Empty;
}
