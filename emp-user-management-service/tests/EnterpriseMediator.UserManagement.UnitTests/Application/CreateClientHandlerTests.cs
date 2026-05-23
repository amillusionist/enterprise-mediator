using EnterpriseMediator.UserManagement.Application.Features.Clients.Commands.CreateClient;
using EnterpriseMediator.UserManagement.Domain.Aggregates.Client;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.UserManagement.UnitTests.Application;

public class CreateClientHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<ILogger<CreateClientHandler>> _loggerMock = new();
    private readonly CreateClientHandler _handler;

    public CreateClientHandlerTests()
    {
        _handler = new CreateClientHandler(_clientRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesClientAndReturnsId()
    {
        _clientRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var command = new CreateClientCommand
        {
            CompanyName = "Test Corp",
            PrimaryContactEmail = "admin@test.com",
            PrimaryContactFirstName = "John",
            PrimaryContactLastName = "Doe",
            AddressLine1 = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "US"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
        _clientRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperation()
    {
        var existingClient = Client.Create("Existing Corp",
            Address.Create("1 St", "City", "ST", "12345", "US"),
            Address.Create("1 St", "City", "ST", "12345", "US"),
            "admin@test.com");

        _clientRepoMock.Setup(r => r.GetByEmailAsync("admin@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingClient);

        var command = new CreateClientCommand
        {
            CompanyName = "Test Corp",
            PrimaryContactEmail = "admin@test.com",
            PrimaryContactFirstName = "John",
            PrimaryContactLastName = "Doe",
            AddressLine1 = "123 Main St",
            City = "New York",
            Country = "US"
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}
