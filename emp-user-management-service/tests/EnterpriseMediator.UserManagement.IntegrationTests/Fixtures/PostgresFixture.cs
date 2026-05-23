using EnterpriseMediator.UserManagement.Infrastructure.Persistence;
using EnterpriseMediator.UserManagement.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;

namespace EnterpriseMediator.UserManagement.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("test_user_db")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public UserDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        var dispatcher = new DomainEventDispatcher(
            new Mock<IPublisher>().Object,
            new Mock<ILogger<DomainEventDispatcher>>().Object);

        DbContext = new UserDbContext(options, dispatcher);
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
