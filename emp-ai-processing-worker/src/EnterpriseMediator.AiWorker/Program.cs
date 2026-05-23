using System.Reflection;
using EnterpriseMediator.AiWorker;
using EnterpriseMediator.AiWorker.Application.Interfaces;
using EnterpriseMediator.AiWorker.Configuration;
using EnterpriseMediator.AiWorker.Features.SowProcessing;
using EnterpriseMediator.AiWorker.Infrastructure.Clients;
using EnterpriseMediator.AiWorker.Infrastructure.Messaging;
using EnterpriseMediator.AiWorker.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

// Initialize bootstrap logger for startup diagnostics
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting EnterpriseMediator.AiWorker host...");

    var builder = Host.CreateApplicationBuilder(args);

    // 1. Configure Serilog
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // 2. Configuration Bindings with Validation
    builder.Services.AddOptions<AiSettings>()
        .Bind(builder.Configuration.GetSection(AiSettings.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddOptions<AwsSettings>()
        .Bind(builder.Configuration.GetSection(AwsSettings.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // 3. Register Application Services (Vertical Slices / Features)
    // Register MediatR for the assembly containing our Command Handlers (Level 2)
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblyContaining<ProcessSowHandler>();
    });

    // Register Validators (Level 1)
    builder.Services.AddSingleton<SowProcessingValidator>();

    // Register Domain Orchestrator (Level 2) - Scoped to the message consumption context
    builder.Services.AddScoped<SowProcessingOrchestrator>();

    // 4. Register AWS Services
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
    builder.Services.AddAWSService<Amazon.Comprehend.IAmazonComprehend>();

    // 5. Register Infrastructure Adapters
    builder.Services.AddSingleton<IAiExtractionService, OpenAiClientAdapter>();
    builder.Services.AddSingleton<IPiiSanitizationService, AwsComprehendAdapter>();
    builder.Services.AddSingleton<IFileStorageService, S3StorageAdapter>();
    builder.Services.AddHttpClient<IDocumentParser, TikaDocumentParser>(client =>
    {
        var tikaUrl = builder.Configuration["Tika:BaseUrl"] ?? "http://localhost:9998";
        client.BaseAddress = new Uri(tikaUrl);
        client.Timeout = TimeSpan.FromMinutes(5);
    });

    // Register Persistence
    builder.Services.AddDbContext<SowDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseNpgsql(connectionString);
    });
    builder.Services.AddScoped<ISowRepository, SowRepository>();

    // Register Event Publisher
    builder.Services.AddScoped<IEventPublisher, EventPublisher>();

    // 5. Configure MassTransit (RabbitMQ)
    builder.Services.AddMassTransit(x =>
    {
        // Register the Consumer from Level 3
        x.AddConsumer<SowUploadedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMq");
            
            cfg.Host(rabbitMqConnectionString);

            // Configure the Receive Endpoint for SOW Processing
            cfg.ReceiveEndpoint("sow-processing-queue", e =>
            {
                // Configure Prefetch Count to manage load on AI services
                // We limit concurrency to prevent hitting OpenAI/AWS Rate Limits (429)
                e.PrefetchCount = 5;
                
                // Retry Policy: Retry transient failures, but fail fast on business logic errors handled in the consumer
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

                // Configure the consumer
                e.ConfigureConsumer<SowUploadedConsumer>(context);
            });

            // Use JSON serializer (default, but explicit is good)
            cfg.ConfigureEndpoints(context);
        });
    });

    // 6. Register the Hosted Worker Service
    // This worker runs alongside MassTransit to provide health logging and process lifecycle management
    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();
    
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "EnterpriseMediator.AiWorker host terminated unexpectedly");
    Environment.Exit(1);
}
finally
{
    Log.CloseAndFlush();
}