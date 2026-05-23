using EnterpriseMediator.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EnterpriseMediator.Financial.IntegrationTests.Infrastructure;

public class FinancialDbContextFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("financial_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public FinancialDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FinancialDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new FinancialDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Apply migrations / ensure schema
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
