using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Emp.ApiGateway.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public IProjectServiceClient ProjectServiceClient { get; } = Substitute.For<IProjectServiceClient>();
    public IFinancialServiceClient FinancialServiceClient { get; } = Substitute.For<IFinancialServiceClient>();
    public IUserServiceClient UserServiceClient { get; } = Substitute.For<IUserServiceClient>();
    public IMessageBus MessageBus { get; } = Substitute.For<IMessageBus>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Remove real HTTP client registrations and replace with mocks
            services.RemoveAll<IProjectServiceClient>();
            services.RemoveAll<IFinancialServiceClient>();
            services.RemoveAll<IUserServiceClient>();
            services.RemoveAll<IMessageBus>();

            services.AddSingleton(ProjectServiceClient);
            services.AddSingleton(FinancialServiceClient);
            services.AddSingleton(UserServiceClient);
            services.AddSingleton(MessageBus);

            // Replace MassTransit with in-memory test harness (no real RabbitMQ)
            services.AddMassTransitTestHarness();
        });
    }
}
