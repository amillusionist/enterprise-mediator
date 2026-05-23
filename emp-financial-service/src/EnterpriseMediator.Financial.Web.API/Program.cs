using System.Text.Json.Serialization;
using EnterpriseMediator.Financial.Application;
using EnterpriseMediator.Financial.Infrastructure;
using EnterpriseMediator.Financial.Infrastructure.Persistence;
using EnterpriseMediator.Financial.Web.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Structured Logging
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
                 .Enrich.FromLogContext()
                 .WriteTo.Console());

// 2. Application Layer Services (MediatR, Validators, Behaviors)
builder.Services.AddApplicationServices();

// 3. Infrastructure Layer Services (DB, Gateways, MassTransit, Repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 4. Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 5. Authentication (AWS Cognito via JWT Bearer)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var region = builder.Configuration["AWS:Region"];
    var userPoolId = builder.Configuration["AWS:UserPoolId"];

    options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateLifetime = true,
#pragma warning disable CA5404 // AWS Cognito access tokens do not include an 'aud' claim
        ValidateAudience = false,
#pragma warning restore CA5404
        ValidateIssuerSigningKey = true,
        RoleClaimType = "cognito:groups"
    };
});

// 6. Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InternalServicePolicy", policy =>
        policy.RequireClaim("scope", "InternalService"));

    options.AddPolicy("RequireFinanceManager", policy =>
        policy.RequireRole("SystemAdministrator", "FinanceManager"));
});

// 7. API Controllers and JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 8. Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Enterprise Mediator Financial API",
        Version = "v1",
        Description = "Microservice for managing Invoices, Payments, Payouts and Ledger."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
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

// --- Build ---
var app = builder.Build();

// --- Middleware Pipeline ---

// Enable Request Buffering for Stripe Webhook Signature Verification
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Auto-migration (Development only)
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FinancialDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating the database.");
    }
}

try
{
    Log.Information("Starting Enterprise Mediator Financial Service...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program public for Integration Tests
public partial class Program { }
