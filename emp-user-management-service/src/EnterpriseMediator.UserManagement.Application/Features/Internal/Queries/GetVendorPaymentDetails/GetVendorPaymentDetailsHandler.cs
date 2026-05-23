using EnterpriseMediator.UserManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Application.Features.Internal.Queries.GetVendorPaymentDetails;

public class GetVendorPaymentDetailsHandler : IRequestHandler<GetVendorPaymentDetailsQuery, PaymentDetailsDto>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<GetVendorPaymentDetailsHandler> _logger;

    public GetVendorPaymentDetailsHandler(
        IVendorRepository vendorRepository,
        ILogger<GetVendorPaymentDetailsHandler> logger)
    {
        _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentDetailsDto> Handle(GetVendorPaymentDetailsQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdAsync(request.VendorId, cancellationToken);
        if (vendor is null)
        {
            throw new KeyNotFoundException($"Vendor with ID '{request.VendorId}' was not found.");
        }

        if (vendor.PaymentDetails is null)
        {
            throw new KeyNotFoundException($"Vendor '{request.VendorId}' has no payment details configured.");
        }

        _logger.LogDebug("Retrieved payment details for vendor {VendorId} via internal API", request.VendorId);

        return new PaymentDetailsDto
        {
            ProviderName = vendor.PaymentDetails.ProviderName,
            AccountIdentifier = vendor.PaymentDetails.AccountIdentifier,
            RoutingIdentifier = vendor.PaymentDetails.RoutingIdentifier,
            Currency = vendor.PaymentDetails.Currency
        };
    }
}
