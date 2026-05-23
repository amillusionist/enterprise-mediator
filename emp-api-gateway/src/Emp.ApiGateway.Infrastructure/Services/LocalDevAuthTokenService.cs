using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Emp.ApiGateway.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Emp.ApiGateway.Infrastructure.Services;

/// <summary>
/// DEV ONLY AUTH BYPASS — issues HS256 JWTs for local development login.
/// </summary>
public sealed class LocalDevAuthTokenService
{
    private readonly LocalDevAuthOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public LocalDevAuthTokenService(IOptions<LocalDevAuthOptions> options)
    {
        _options = options.Value;
    }

    public string CreateAccessToken(LocalDevAuthUserOptions user)
    {
        var now = DateTime.UtcNow;
        var claims = BuildClaims(user);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_options.AccessTokenExpirationMinutes),
            signingCredentials: SigningCredentials);

        return _tokenHandler.WriteToken(token);
    }

    public string CreateRefreshToken(LocalDevAuthUserOptions user)
    {
        var now = DateTime.UtcNow;
        var claims = BuildClaims(user).Append(new Claim("token_use", "refresh"));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddDays(_options.RefreshTokenExpirationDays),
            signingCredentials: SigningCredentials);

        return _tokenHandler.WriteToken(token);
    }

    public bool TryValidateRefreshToken(string refreshToken, out LocalDevAuthUserOptions? user)
    {
        user = null;
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        try
        {
            _tokenHandler.ValidateToken(
                refreshToken,
                ValidationParameters,
                out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt
                || !string.Equals(jwt.Claims.FirstOrDefault(c => c.Type == "token_use")?.Value, "refresh", StringComparison.Ordinal))
            {
                return false;
            }

            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            user = _options.Users.FirstOrDefault(u =>
                string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
            return user is not null;
        }
        catch
        {
            return false;
        }
    }

    public bool IsLocalDevAccessToken(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        try
        {
            var jwt = _tokenHandler.ReadJwtToken(accessToken);
            return string.Equals(jwt.Issuer, _options.Issuer, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private IEnumerable<Claim> BuildClaims(LocalDevAuthUserOptions user)
    {
        var userId = DeterministicUserId(user.Email);

        return
        [
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("cognito:groups", user.Role),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("tenantId", user.TenantId),
            new Claim("permissions", "projects:read,projects:write,users:read,finance:read,vendors:read,clients:read"),
        ];
    }

    private static string DeterministicUserId(string email)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return new Guid(bytes).ToString();
    }

    private SigningCredentials SigningCredentials =>
        new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)), SecurityAlgorithms.HmacSha256);

    private TokenValidationParameters ValidationParameters => new()
    {
        ValidIssuer = _options.Issuer,
        ValidAudience = _options.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1),
    };
}
