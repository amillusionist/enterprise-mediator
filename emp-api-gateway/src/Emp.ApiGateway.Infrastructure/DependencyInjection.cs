using Amazon;
using Amazon.CognitoIdentityProvider;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using Emp.ApiGateway.Infrastructure.Configuration;
using Emp.ApiGateway.Infrastructure.Messaging;
using Emp.ApiGateway.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Emp.ApiGateway.Infrastructure
{
    /// <summary>
    /// Provides extension methods for registering Infrastructure layer services.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers Infrastructure services including HTTP clients, Messaging, and Configuration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration manager.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Configuration Options
            services.Configure<ServiceUrls>(configuration.GetSection(ServiceUrls.SectionName));
            services.Configure<AwsCognitoSettings>(configuration.GetSection("AWS:Cognito"));

            // Register Typed HTTP Clients with Resilience Policies
            services.AddHttpClient<IProjectServiceClient, ProjectServiceClient>((serviceProvider, client) =>
            {
                var serviceUrls = serviceProvider.GetRequiredService<IOptions<ServiceUrls>>().Value;
                
                if (string.IsNullOrEmpty(serviceUrls.ProjectService))
                {
                    throw new InvalidOperationException("ProjectService URL is not configured.");
                }

                client.BaseAddress = new Uri(serviceUrls.ProjectService);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddStandardResilienceHandler(); // Applies retry, circuit breaker, and timeout policies

            services.AddHttpClient<IFinancialServiceClient, FinancialServiceClient>((serviceProvider, client) =>
            {
                var serviceUrls = serviceProvider.GetRequiredService<IOptions<ServiceUrls>>().Value;

                if (string.IsNullOrEmpty(serviceUrls.FinancialService))
                {
                    throw new InvalidOperationException("FinancialService URL is not configured.");
                }

                client.BaseAddress = new Uri(serviceUrls.FinancialService);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddStandardResilienceHandler();

            services.AddHttpClient<IUserServiceClient, UserServiceClient>((serviceProvider, client) =>
            {
                var serviceUrls = serviceProvider.GetRequiredService<IOptions<ServiceUrls>>().Value;

                if (string.IsNullOrEmpty(serviceUrls.UserService))
                {
                    throw new InvalidOperationException("UserService URL is not configured.");
                }

                client.BaseAddress = new Uri(serviceUrls.UserService);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddStandardResilienceHandler();

            services.AddHttpClient<IAuditServiceClient, AuditServiceClient>((serviceProvider, client) =>
            {
                var serviceUrls = serviceProvider.GetRequiredService<IOptions<ServiceUrls>>().Value;

                if (string.IsNullOrEmpty(serviceUrls.AuditService))
                {
                    throw new InvalidOperationException("AuditService URL is not configured.");
                }

                client.BaseAddress = new Uri(serviceUrls.AuditService);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddStandardResilienceHandler();

            // Register AWS Cognito Identity Provider Client (production path — region required if invoked)
            services.AddSingleton(sp =>
            {
                var regionName = configuration["AWS:Region"] ?? "us-east-1";
                return new AmazonCognitoIdentityProviderClient(RegionEndpoint.GetBySystemName(regionName));
            });
            services.Configure<LocalDevAuthOptions>(configuration.GetSection(LocalDevAuthOptions.SectionName));
            services.AddScoped<CognitoAuthService>();
            services.AddScoped<LocalDevAuthTokenService>();
            services.AddScoped<ICognitoAuthService>(sp =>
            {
                var environment = sp.GetRequiredService<IHostEnvironment>();
                var localDevOptions = sp.GetRequiredService<IOptions<LocalDevAuthOptions>>().Value;
                var cognito = sp.GetRequiredService<CognitoAuthService>();

                // DEV ONLY AUTH BYPASS
                if (environment.IsDevelopment()
                    && localDevOptions.Enabled
                    && localDevOptions.SigningKey.Length >= 32)
                {
                    return new LocalDevAuthServiceDecorator(
                        cognito,
                        sp.GetRequiredService<LocalDevAuthTokenService>(),
                        sp.GetRequiredService<IOptions<LocalDevAuthOptions>>(),
                        environment,
                        sp.GetRequiredService<ILogger<LocalDevAuthServiceDecorator>>());
                }

                return cognito;
            });

            // Register MassTransit for Asynchronous Messaging
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitHost = configuration["RabbitMQ:Host"] ?? "localhost";
                    var rabbitVHost = configuration["RabbitMQ:VirtualHost"] ?? "/";
                    var rabbitUser = configuration["RabbitMQ:Username"] ?? "guest";
                    var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";

                    cfg.Host(rabbitHost, rabbitVHost, h =>
                    {
                        h.Username(rabbitUser);
                        h.Password(rabbitPass);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            // Register Services
            services.AddScoped<IMessageBus, MassTransitPublisher>();

            return services;
        }
    }
}