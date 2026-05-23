using EnterpriseMediator.UserManagement.Domain.Interfaces;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Application.Features.Vendors.Commands.UpdateProfile;

public class UpdateVendorProfileHandler : IRequestHandler<UpdateVendorProfileCommand, bool>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<UpdateVendorProfileHandler> _logger;

    public UpdateVendorProfileHandler(IVendorRepository vendorRepository, ILogger<UpdateVendorProfileHandler> logger)
    {
        _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(UpdateVendorProfileCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdAsync(request.VendorId, cancellationToken);
        if (vendor is null)
        {
            throw new KeyNotFoundException($"Vendor with ID '{request.VendorId}' was not found.");
        }

        if (request.CompanyName is not null && request.AddressLine1 is not null)
        {
            var address = Address.Create(
                request.AddressLine1,
                request.City ?? vendor.Address?.City ?? string.Empty,
                request.State ?? vendor.Address?.State ?? string.Empty,
                request.PostalCode ?? vendor.Address?.PostalCode ?? string.Empty,
                request.Country ?? vendor.Address?.Country ?? string.Empty);

            vendor.UpdateProfile(
                request.CompanyName,
                address,
                vendor.PrimaryContactEmail);
        }
        else if (request.CompanyName is not null)
        {
            vendor.UpdateProfile(
                request.CompanyName,
                vendor.Address!,
                vendor.PrimaryContactEmail);
        }

        if (request.Skills is not null)
        {
            vendor.UpdateSkills(request.Skills);
        }

        if (request.PaymentProvider is not null && request.PaymentAccountNumber is not null)
        {
            var paymentInfo = PaymentInfo.Create(
                request.PaymentProvider,
                request.PaymentAccountNumber,
                "USD",
                request.PaymentRoutingNumber);

            vendor.UpdatePaymentDetails(paymentInfo);
        }

        _vendorRepository.Update(vendor);
        await _vendorRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vendor {VendorId} profile updated", request.VendorId);

        return true;
    }
}
