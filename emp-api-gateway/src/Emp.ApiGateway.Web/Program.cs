using Emp.ApiGateway.Application;
using Emp.ApiGateway.Infrastructure;
using Emp.ApiGateway.Infrastructure.Configuration;
using Emp.ApiGateway.Web.Extensions;
using Emp.ApiGateway.Web.Middleware;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;

// 1. Bootstrap Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Emp.ApiGateway.Web...");

    var builder = WebApplication.CreateBuilder(args);

    // 2. Configure Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // 3. Add Services to the Container

    // A. Configuration Bindings
    builder.Services.Configure<ServiceUrls>(builder.Configuration.GetSection("ServiceUrls"));
    builder.Services.Configure<AwsCognitoSettings>(builder.Configuration.GetSection("AWS:Cognito"));

    // B. Framework Services (Controllers, JSON options)
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddHealthChecks();

    // C. Layer Dependencies (Clean Architecture)
    // Registers Application layer services (MediatR, Mappers, etc.)
    builder.Services.AddApplication();
    // Registers Infrastructure layer services (HttpClients, MassTransit, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

    // D. Custom Middleware and Exception Handling
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddScoped<CorrelationIdMiddleware>();

    // E. Authentication & Authorization (AWS Cognito + DEV ONLY local JWT bypass in Development)
    builder.Services.AddEmpAuthentication(builder.Configuration, builder.Environment);
    if (builder.Environment.IsDevelopment()
        && builder.Configuration.GetValue<bool>("LocalDevAuth:Enabled"))
    {
        Log.Warning("DEV ONLY AUTH BYPASS is enabled — local demo accounts can authenticate without Cognito.");
    }

    builder.Services.AddAuthorization(options =>
    {
        // Define specific policies here if needed, e.g., for specific scopes or groups
        // options.AddPolicy("AdminOnly", policy => policy.RequireClaim("cognito:groups", "Admin"));
    });

    // F. Swagger / OpenAPI Configuration
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "Enterprise Mediator Platform - API Gateway", 
            Version = "v1",
            Description = "BFF (Backend for Frontend) Gateway for EMP."
        });

        // Configure JWT Auth for Swagger UI
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // G. CORS Configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // H. Rate Limiting
    builder.Services.AddCustomRateLimiting(builder.Configuration);

    // 4. Build Application
    var app = builder.Build();

    // 5. Configure Middleware Pipeline
    // Order is critical here

    // A. Error Handling (First to catch everything)
    app.UseExceptionHandler();

    // B. Observability
    app.UseSerilogRequestLogging();
    app.UseMiddleware<CorrelationIdMiddleware>();

    // C. Security Headers & Https
    app.UseHttpsRedirection();
    
    // D. Swagger (Only in Dev/Staging usually, but keeping active for visibility in this context)
    if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Staging")
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // E. CORS
    app.UseCors("AllowFrontend");

    // E2. Rate Limiting
    app.UseCustomRateLimiting();

    // F. Auth
    app.UseAuthentication();
    app.UseAuthorization();

    // G. Routing & Endpoints
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Log startup
    Log.Information("Application configuration complete. Starting host...");
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Enables WebApplicationFactory<Program> access for integration tests
public partial class Program { }