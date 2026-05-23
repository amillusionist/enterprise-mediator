using Amazon.CognitoIdentityProvider;
using EnterpriseMediator.UserManagement.Application.Interfaces;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using EnterpriseMediator.UserManagement.Infrastructure.Persistence;
using EnterpriseMediator.UserManagement.Infrastructure.Persistence.Repositories;
using EnterpriseMediator.UserManagement.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseMediator.UserManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context (PostgreSQL)
        services.AddDbContext<UserDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("EnterpriseMediator.UserManagement.Infrastructure");
                npgsqlOptions.EnableRetryOnFailure(3);
            });
        });

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();

        // Infrastructure Services
        services.AddScoped<IAuditServiceAdapter, AuditServiceAdapter>();
        services.AddScoped<IIdentityService, CognitoIdentityService>();
        services.AddScoped<DomainEventDispatcher>();

        // AWS Cognito
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonCognitoIdentityProvider>();

        // MassTransit (Message Bus)
        services.AddMassTransit(busConfig =>
        {
            busConfig.SetKebabCaseEndpointNameFormatter();

            busConfig.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitUser = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";
                var virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                cfg.Host(rabbitHost, virtualHost, h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
