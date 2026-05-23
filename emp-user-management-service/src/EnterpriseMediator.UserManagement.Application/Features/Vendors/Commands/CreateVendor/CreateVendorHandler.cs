using EnterpriseMediator.UserManagement.Domain.Aggregates.Vendor;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Application.Features.Vendors.Commands.CreateVendor;

public class CreateVendorHandler : IRequestHandler<CreateVendorCommand, Guid>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<CreateVendorHandler> _logger;

    public CreateVendorHandler(IVendorRepository vendorRepository, ILogger<CreateVendorHandler> logger)
    {
        _vendorRepository = vendorRepository ?? throw new ArgumentNullException(nameof(vendorRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        var existingVendor = await _vendorRepository.GetByEmailAsync(request.PrimaryContactEmail, cancellationToken);
        if (existingVendor is not null)
        {
            throw new InvalidOperationException($"A vendor with email '{request.PrimaryContactEmail}' already exists.");
        }

        var address = Address.Create(
            request.AddressLine1,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var vendor = Vendor.Create(
            request.CompanyName,
            address,
            request.PrimaryContactEmail);

        if (request.Skills.Count > 0)
        {
            vendor.UpdateSkills(request.Skills);
        }

        await _vendorRepository.AddAsync(vendor, cancellationToken);
        await _vendorRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vendor {VendorId} created: {CompanyName}", vendor.Id, vendor.CompanyName);

        return vendor.Id;
    }
}
