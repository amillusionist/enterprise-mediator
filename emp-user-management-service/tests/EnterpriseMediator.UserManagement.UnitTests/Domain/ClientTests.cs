using EnterpriseMediator.UserManagement.Domain.Aggregates.Client;
using EnterpriseMediator.UserManagement.Domain.Events;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using FluentAssertions;

namespace EnterpriseMediator.UserManagement.UnitTests.Domain;

public class ClientTests
{
    private static Address CreateTestAddress() =>
        Address.Create("123 Main St", "New York", "NY", "10001", "US");

    [Fact]
    public void Create_ValidInput_ReturnsClientWithCorrectProperties()
    {
        var client = Client.Create("BigCo Inc", CreateTestAddress(), CreateTestAddress(), "admin@bigco.com");

        client.CompanyName.Should().Be("BigCo Inc");
        client.PrimaryContactEmail.Should().Be("admin@bigco.com");
        client.Status.Should().Be("Active");
        client.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_RaisesClientCreatedEvent()
    {
        var client = Client.Create("BigCo Inc", CreateTestAddress(), CreateTestAddress(), "admin@bigco.com");

        client.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ClientCreatedEvent>();
    }

    [Fact]
    public void UpdateDetails_UpdatesCompanyNameAndEmail()
    {
        var client = Client.Create("BigCo Inc", CreateTestAddress(), CreateTestAddress(), "admin@bigco.com");

        client.UpdateDetails("NewCo LLC", "newadmin@newco.com");

        client.CompanyName.Should().Be("NewCo LLC");
        client.PrimaryContactEmail.Should().Be("newadmin@newco.com");
    }

    [Fact]
    public void Deactivate_ActiveClient_SetsInactive()
    {
        var client = Client.Create("BigCo Inc", CreateTestAddress(), CreateTestAddress(), "admin@bigco.com");

        client.Deactivate();

        client.Status.Should().Be("Inactive");
    }

    [Fact]
    public void Activate_InactiveClient_SetsActive()
    {
        var client = Client.Create("BigCo Inc", CreateTestAddress(), CreateTestAddress(), "admin@bigco.com");
        client.Deactivate();

        client.Activate();

        client.Status.Should().Be("Active");
    }
}
