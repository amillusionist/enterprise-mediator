using EnterpriseMediator.UserManagement.Domain.Aggregates.Client;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using EnterpriseMediator.UserManagement.Infrastructure.Persistence.Repositories;
using EnterpriseMediator.UserManagement.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.UserManagement.IntegrationTests.Repositories;

public class ClientRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;
    private readonly ClientRepository _repository;

    public ClientRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _repository = new ClientRepository(
            fixture.DbContext,
            new Mock<ILogger<ClientRepository>>().Object);
    }

    [Fact]
    public async Task AddAndRetrieveClient_ById_ReturnsClient()
    {
        var address = Address.Create("100 Client Ave", "Chicago", "IL", "60601", "US");
        var client = Client.Create("Integration Test Client", address, address, "client-int@test.com");

        await _repository.AddAsync(client);
        await _repository.SaveChangesAsync();

        var retrieved = await _repository.GetByIdAsync(client.Id);

        retrieved.Should().NotBeNull();
        retrieved!.CompanyName.Should().Be("Integration Test Client");
        retrieved.PrimaryContactEmail.Should().Be("client-int@test.com");
    }

    [Fact]
    public async Task ListAsync_ReturnsPaginatedResults()
    {
        var address = Address.Create("200 List St", "Denver", "CO", "80201", "US");
        var client = Client.Create("List Test Client", address, address, "list-test@client.com");

        await _repository.AddAsync(client);
        await _repository.SaveChangesAsync();

        var results = await _repository.ListAsync(1, 10);

        results.Should().NotBeEmpty();
    }
}
