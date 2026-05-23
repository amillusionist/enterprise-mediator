using EnterpriseMediator.UserManagement.Domain.Aggregates.Vendor;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using EnterpriseMediator.UserManagement.Infrastructure.Persistence.Repositories;
using EnterpriseMediator.UserManagement.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.UserManagement.IntegrationTests.Repositories;

public class VendorRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;
    private readonly VendorRepository _repository;

    public VendorRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _repository = new VendorRepository(
            fixture.DbContext,
            new Mock<ILogger<VendorRepository>>().Object);
    }

    [Fact]
    public async Task AddAndRetrieveVendor_ById_ReturnsVendorWithSkills()
    {
        var address = Address.Create("123 Tech Way", "Austin", "TX", "73301", "US");
        var vendor = Vendor.Create("Integration Test Vendor", address, "vendor-int@test.com");
        vendor.UpdateSkills(new[] { "C#", ".NET Core" });

        await _repository.AddAsync(vendor);
        await _repository.SaveChangesAsync();

        var retrieved = await _repository.GetByIdAsync(vendor.Id);

        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Integration Test Vendor");
        retrieved.Skills.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByEmail_ExistingVendor_ReturnsVendor()
    {
        var address = Address.Create("456 Dev Blvd", "Seattle", "WA", "98101", "US");
        var vendor = Vendor.Create("Email Test Vendor", address, "vendoremail@test.com");

        await _repository.AddAsync(vendor);
        await _repository.SaveChangesAsync();

        var retrieved = await _repository.GetByEmailAsync("vendoremail@test.com");

        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Email Test Vendor");
    }

    [Fact]
    public async Task ListByStatus_ReturnsMatchingVendors()
    {
        var address = Address.Create("789 Status St", "Portland", "OR", "97201", "US");
        var vendor = Vendor.Create("Status Test Vendor", address, "status-test@vendor.com");

        await _repository.AddAsync(vendor);
        await _repository.SaveChangesAsync();

        var results = await _repository.ListByStatusAsync("Pending", 1, 10);

        results.Should().NotBeEmpty();
    }
}
