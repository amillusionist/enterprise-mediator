using EnterpriseMediator.UserManagement.API.Extensions;
using EnterpriseMediator.UserManagement.API.Middleware;
using EnterpriseMediator.UserManagement.API.Services;
using EnterpriseMediator.UserManagement.Application;
using EnterpriseMediator.UserManagement.Application.Interfaces;
using EnterpriseMediator.UserManagement.Infrastructure;
using Serilog;

// Bootstrap Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

    // Application Layer (MediatR, FluentValidation, Config)
    builder.Services.AddApplication(builder.Configuration);

    // Infrastructure Layer (EF Core, Repositories, MassTransit, Cognito)
    builder.Services.AddInfrastructure(builder.Configuration);

    // CurrentUserService (wraps HttpContext for auth claims)
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // Authentication (AWS Cognito + DEV ONLY local JWT bypass in Development)
    builder.Services.AddEmpJwtAuthentication(builder.Configuration, builder.Environment);

    // Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("InternalServicePolicy", policy =>
            policy.RequireClaim("scope", "InternalService"));

        options.AddPolicy("RequireAdmin", policy =>
            policy.RequireRole("SystemAdmin"));
    });

    // Global Exception Handling
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddControllers();

    // Swagger / OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "EnterpriseMediator User Management API",
            Version = "v1"
        });

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Build the application
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
        .AllowAnonymous();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
