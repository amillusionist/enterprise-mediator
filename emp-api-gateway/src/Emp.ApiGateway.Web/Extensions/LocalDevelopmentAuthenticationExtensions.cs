using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Emp.ApiGateway.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Emp.ApiGateway.Web.Extensions;

/// <summary>
/// DEV ONLY AUTH BYPASS — dual JWT validation (local HS256 + AWS Cognito) in Development only.
/// </summary>
public static class LocalDevelopmentAuthenticationExtensions
{
    public const string CognitoScheme = "Cognito";
    public const string LocalDevScheme = "LocalDev";
    public const string SmartBearerScheme = "SmartBearer";

    public static IServiceCollection AddEmpAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var localDevSection = configuration.GetSection(LocalDevAuthOptions.SectionName);
        services.Configure<LocalDevAuthOptions>(localDevSection);

        var localDevOptions = localDevSection.Get<LocalDevAuthOptions>() ?? new LocalDevAuthOptions();
        var enableLocalDev = environment.IsDevelopment()
            && localDevOptions.Enabled
            && localDevOptions.SigningKey.Length >= 32;

        var cognitoSettings = configuration.GetSection("AWS:Cognito").Get<AwsCognitoSettings>();
        var region = configuration["AWS:Region"] ?? "us-east-1";
        var userPoolId = cognitoSettings?.UserPoolId ?? string.Empty;

        var authBuilder = services.AddAuthentication(options =>
        {
            if (enableLocalDev)
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

        authBuilder.AddJwtBearer(enableLocalDev ? CognitoScheme : JwtBearerDefaults.AuthenticationScheme, options =>
        {
            if (!string.IsNullOrWhiteSpace(userPoolId))
            {
                options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}",
#pragma warning disable CA5404 // AWS Cognito access tokens do not include an 'aud' claim
                    ValidateAudience = false,
#pragma warning restore CA5404
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RoleClaimType = "cognito:groups",
                    ClockSkew = TimeSpan.Zero,
                };
            }
            else if (!enableLocalDev)
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = false,
                };
            }
        });

        if (enableLocalDev)
        {
            authBuilder.AddJwtBearer(LocalDevScheme, options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = localDevOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = localDevOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(localDevOptions.SigningKey)),
                    RoleClaimType = "cognito:groups",
                    NameClaimType = "email",
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

            authBuilder.AddPolicyScheme(SmartBearerScheme, "Authorization Bearer", policyOptions =>
            {
                policyOptions.ForwardDefaultSelector = context =>
                {
                    var authorization = context.Request.Headers.Authorization.ToString();
                    if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        return CognitoScheme;
                    }

                    var token = authorization["Bearer ".Length..].Trim();
                    try
                    {
                        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                        if (string.Equals(jwt.Issuer, localDevOptions.Issuer, StringComparison.Ordinal))
                        {
                            return LocalDevScheme;
                        }
                    }
                    catch
                    {
                        // Fall through to Cognito
                    }

                    return CognitoScheme;
                };
            });
        }

        return services;
    }
}
