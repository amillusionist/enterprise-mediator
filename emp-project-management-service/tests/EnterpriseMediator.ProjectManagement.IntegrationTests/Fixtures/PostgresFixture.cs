using EnterpriseMediator.ProjectManagement.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace EnterpriseMediator.ProjectManagement.IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .WithDatabase("emp_project_management_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public ProjectDbContext CreateDbContext()
    {
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ProjectDbContext>());

        var provider = services.BuildServiceProvider();

        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(ConnectionString, npgsql =>
            {
                npgsql.UseVector();
            })
            .Options;

        return new ProjectDbContext(options, provider.GetRequiredService<IPublisher>());
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateDbContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
