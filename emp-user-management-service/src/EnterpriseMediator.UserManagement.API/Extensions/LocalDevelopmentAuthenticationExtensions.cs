using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace EnterpriseMediator.UserManagement.API.Extensions;

/// <summary>
/// DEV ONLY AUTH BYPASS — validates local development JWTs alongside Cognito in Development.
/// </summary>
public static class LocalDevelopmentAuthenticationExtensions
{
    private const string CognitoScheme = "Cognito";
    private const string LocalDevScheme = "LocalDev";
    private const string SmartBearerScheme = "SmartBearer";

    public static IServiceCollection AddEmpJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var localDevSection = configuration.GetSection("LocalDevAuth");
        var enabled = environment.IsDevelopment()
            && localDevSection.GetValue<bool>("Enabled")
            && (localDevSection["SigningKey"]?.Length ?? 0) >= 32;

        var region = configuration["AWS:Region"] ?? "us-east-1";
        var userPoolId = configuration["AWS:UserPoolId"] ?? string.Empty;

        var authBuilder = services.AddAuthentication(options =>
        {
            if (enabled)
            {
                options.DefaultAuthenticateScheme = SmartBearerScheme;
                options.DefaultChallengeScheme = SmartBearerScheme;
            }
            else
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
        });

        authBuilder.AddJwtBearer(enabled ? CognitoScheme : JwtBearerDefaults.AuthenticationScheme, options =>
        {
            if (!string.IsNullOrWhiteSpace(userPoolId))
            {
                options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateLifetime = true,
#pragma warning disable CA5404 // AWS Cognito access tokens do not include an 'aud' claim
                    ValidateAudience = false,
#pragma warning restore CA5404
                    ValidateIssuerSigningKey = true,
                    RoleClaimType = "cognito:groups",
                };
            }
        });

        if (enabled)
        {
            var issuer = localDevSection["Issuer"] ?? "https://localhost/emp-dev-auth";
            var audience = localDevSection["Audience"] ?? "emp-platform-local";
            var signingKey = localDevSection["SigningKey"]!;

            authBuilder.AddJwtBearer(LocalDevScheme, options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RoleClaimType = "cognito:groups",
                    NameClaimType = "email",
                };
            });

            authBuilder.AddPolicyScheme(SmartBearerScheme, SmartBearerScheme, policy =>
            {
                policy.ForwardDefaultSelector = context =>
                {
                    var header = context.Request.Headers.Authorization.ToString();
                    if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = header["Bearer ".Length..].Trim();
                        try
                        {
                            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                            if (string.Equals(jwt.Issuer, issuer, StringComparison.Ordinal))
                            {
                                return LocalDevScheme;
                            }
                        }
                        catch
                        {
                            // use Cognito
                        }
                    }

                    return CognitoScheme;
                };
            });
        }

        return services;
    }
}
