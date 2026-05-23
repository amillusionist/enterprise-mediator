using EnterpriseMediator.Financial.Domain.Interfaces;
using EnterpriseMediator.Financial.Infrastructure.Gateways.Stripe;
using EnterpriseMediator.Financial.Infrastructure.Gateways.Wise;
using EnterpriseMediator.Financial.Infrastructure.Interceptors;
using EnterpriseMediator.Financial.Infrastructure.Messaging.Consumers;
using EnterpriseMediator.Financial.Infrastructure.Persistence;
using EnterpriseMediator.Financial.Infrastructure.Persistence.Configurations;
using EnterpriseMediator.Financial.Infrastructure.Persistence.Repositories;
using EnterpriseMediator.Financial.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseMediator.Financial.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration Settings
        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));
        services.Configure<WiseSettings>(configuration.GetSection(WiseSettings.SectionName));

        // Audit Interceptor
        services.AddScoped<ISaveChangesInterceptor, FinancialAuditInterceptor>();

        // Database Context
        services.AddDbContextPool<FinancialDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(FinancialDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(3);
            });

            options.AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>());
        });

        // Repositories
        services.AddScoped<IFinancialRepository, FinancialRepository>();
        services.AddScoped<ITransactionQueryRepository, TransactionQueryRepository>();

        // DateTime Provider
        services.AddSingleton<IDateTime, DateTimeProvider>();

        // Payment Gateway
        services.AddScoped<IPaymentGateway, StripePaymentAdapter>();

        // Payout Gateway (HttpClient factory)
        services.AddHttpClient<IPayoutGateway, WisePayoutAdapter>();

        // MassTransit
        services.AddMassTransit(busConfig =>
        {
            busConfig.SetKebabCaseEndpointNameFormatter();

            busConfig.AddConsumer<ProjectAwardedConsumer>();
            busConfig.AddConsumer<MilestoneApprovedConsumer>();

            busConfig.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RabbitMq:Host"] ?? "localhost";
                var rabbitMqUser = configuration["RabbitMq:Username"] ?? "guest";
                var rabbitMqPass = configuration["RabbitMq:Password"] ?? "guest";
                var virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";

                cfg.Host(rabbitMqHost, virtualHost, h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPass);
                });

                cfg.UseMessageRetry(r => r.Exponential(
                    retryLimit: 5,
                    minInterval: TimeSpan.FromSeconds(1),
                    maxInterval: TimeSpan.FromSeconds(30),
                    intervalDelta: TimeSpan.FromSeconds(5)));

                cfg.ConfigureEndpoints(context);
            });
        });

        // Health Checks
        services.AddHealthChecks();

        return services;
    }
}
