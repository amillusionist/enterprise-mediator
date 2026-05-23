using EnterpriseMediator.UserManagement.Application.Features.Vendors.Commands.CreateVendor;
using EnterpriseMediator.UserManagement.Domain.Aggregates.Vendor;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.UserManagement.UnitTests.Application;

public class CreateVendorHandlerTests
{
    private readonly Mock<IVendorRepository> _vendorRepoMock = new();
    private readonly Mock<ILogger<CreateVendorHandler>> _loggerMock = new();
    private readonly CreateVendorHandler _handler;

    public CreateVendorHandlerTests()
    {
        _handler = new CreateVendorHandler(_vendorRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesVendorAndReturnsId()
    {
        _vendorRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vendor?)null);

        var command = new CreateVendorCommand
        {
            CompanyName = "Acme Consulting",
            PrimaryContactEmail = "vendor@acme.com",
            PrimaryContactFirstName = "Jane",
            PrimaryContactLastName = "Smith",
            AddressLine1 = "456 Oak Ave",
            City = "San Francisco",
            State = "CA",
            PostalCode = "94102",
            Country = "US",
            Skills = new List<string> { "C#", ".NET", "Azure" }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _vendorRepoMock.Verify(r => r.AddAsync(It.IsAny<Vendor>(), It.IsAny<CancellationToken>()), Times.Once);
        _vendorRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperation()
    {
        var existingVendor = Vendor.Create("Existing Vendor",
            Address.Create("1 St", "City", "ST", "12345", "US"),
            "vendor@acme.com");

        _vendorRepoMock.Setup(r => r.GetByEmailAsync("vendor@acme.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingVendor);

        var command = new CreateVendorCommand
        {
            CompanyName = "Acme Consulting",
            PrimaryContactEmail = "vendor@acme.com",
            PrimaryContactFirstName = "Jane",
            PrimaryContactLastName = "Smith",
            AddressLine1 = "456 Oak Ave",
            City = "San Francisco",
            Country = "US"
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}
