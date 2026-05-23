using System.Reflection;
using EnterpriseMediator.Core.SharedKernel.Abstractions;
using EnterpriseMediator.Core.SharedKernel.Behaviors;
using EnterpriseMediator.Core.SharedKernel.Common;
using EnterpriseMediator.Core.SharedKernel.Configuration;
using EnterpriseMediator.Core.SharedKernel.Implementations;
using EnterpriseMediator.Core.SharedKernel.Implementations.Data;
using EnterpriseMediator.Core.SharedKernel.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnterpriseMediator.Core.SharedKernel.Extensions;

/// <summary>
/// Provides extension methods for IServiceCollection to register Shared Kernel components.
/// This acts as the Composition Root helper for the shared infrastructure layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all core Shared Kernel services, behaviors, and configurations.
    /// </summary>
    public static IServiceCollection AddSharedKernel(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assembliesToScan)
    {
        // 1. Configuration Options
        services.Configure<SharedKernelOptions>(configuration.GetSection(SharedKernelOptions.SectionName));
        services.Configure<SerilogOptions>(configuration.GetSection(SharedKernelOptions.SerilogSectionName));
        services.Configure<ResiliencyOptions>(configuration.GetSection(SharedKernelOptions.ResiliencySectionName));

        // 2. Core Services
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddTransient<GlobalExceptionHandler>();

        // 3. Repository (open-generic, consumers must register their DbContext)
        services.TryAddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
        services.TryAddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        // 4. MediatR Pipeline Behaviors
        // Order: Logging (outer) → Performance → Validation (inner) → Handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // 5. FluentValidation auto-registration
        if (assembliesToScan is { Length: > 0 })
        {
            services.AddValidatorsFromAssemblies(assembliesToScan, ServiceLifetime.Transient);
        }

        // 6. Polly resiliency policies for HttpClient
        services.AddResiliencyPolicies(configuration);

        return services;
    }

    /// <summary>
    /// Registers Polly retry and circuit breaker policies as a named HttpClient policy set.
    /// Services can apply these to their typed HttpClients.
    /// </summary>
    public static IServiceCollection AddResiliencyPolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var resiliencyOptions = new ResiliencyOptions();
        configuration.GetSection(SharedKernelOptions.ResiliencySectionName).Bind(resiliencyOptions);

        services.AddHttpClient("ResilientClient")
            .AddPolicyHandler(PollyPolicyExtensions.GetRetryPolicy(resiliencyOptions))
            .AddPolicyHandler(PollyPolicyExtensions.GetCircuitBreakerPolicy(resiliencyOptions));

        return services;
    }
}