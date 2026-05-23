using EnterpriseMediator.UserManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Application.Features.Vendors.Queries.GetVendorDetails;

public class GetVendorDetailsHandler : IRequestHandler<GetVendorDetailsQuery, VendorDetailsDto>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<GetVendorDetailsHandler> _logger;

    public GetVendorDetailsHandler(IVendorRepository vendorRepository, ILogger<GetVendorDetailsHandler> logger)
    {
        _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VendorDetailsDto> Handle(GetVendorDetailsQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdAsync(request.VendorId, cancellationToken);
        if (vendor is null)
        {
            throw new KeyNotFoundException($"Vendor with ID '{request.VendorId}' was not found.");
        }

        _logger.LogDebug("Retrieved vendor details for {VendorId}", request.VendorId);

        return new VendorDetailsDto
        {
            Id = vendor.Id,
            CompanyName = vendor.CompanyName,
            PrimaryContactEmail = vendor.PrimaryContactEmail,
            VettingStatus = vendor.VettingStatus,
            CreatedAt = vendor.CreatedAt,
            Address = vendor.Address is not null ? new VendorAddressDto
            {
                Street = vendor.Address.Street,
                City = vendor.Address.City,
                State = vendor.Address.State,
                PostalCode = vendor.Address.PostalCode,
                Country = vendor.Address.Country
            } : null,
            PaymentDetails = vendor.PaymentDetails is not null ? new VendorPaymentInfoDto
            {
                ProviderName = vendor.PaymentDetails.ProviderName,
                MaskedAccountIdentifier = vendor.PaymentDetails.GetMaskedDetails(),
                Currency = vendor.PaymentDetails.Currency
            } : null,
            Skills = vendor.Skills.Select(s => s.Name).ToList()
        };
    }
}
