using Amazon.S3;
using EnterpriseMediator.ProjectManagement.Application.Configuration;
using EnterpriseMediator.ProjectManagement.Application.Interfaces;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using EnterpriseMediator.ProjectManagement.Domain.Services;
using EnterpriseMediator.ProjectManagement.Infrastructure.Gateways;
using EnterpriseMediator.ProjectManagement.Infrastructure.Messaging;
using EnterpriseMediator.ProjectManagement.Infrastructure.Persistence;
using EnterpriseMediator.ProjectManagement.Infrastructure.Persistence.Repositories;
using EnterpriseMediator.ProjectManagement.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseMediator.ProjectManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string DefaultConnection not found.");

        services.AddDbContext<ProjectDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseVector();
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            });
        });

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IVendorMatchingService, VectorVendorMatchingService>();
        services.AddScoped<IMessageBus, MassTransitMessageBus>();

        services.Configure<AwsOptions>(configuration.GetSection(AwsOptions.SectionName));
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonS3>();
        services.AddScoped<IFileStorageService, S3FileStorageService>();

        var rabbitMqSection = configuration.GetSection(RabbitMqOptions.SectionName);
        var rabbitHost = rabbitMqSection["Host"] ?? "localhost";
        var rabbitUser = rabbitMqSection["Username"] ?? "guest";
        var rabbitPass = rabbitMqSection["Password"] ?? "guest";

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitHost, "/", h =>
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
