namespace Emp.ApiGateway.Infrastructure.Configuration;

/// <summary>
/// DEV ONLY AUTH BYPASS — local Docker / Development credentials. Never enable in Production.
/// </summary>
public sealed class LocalDevAuthOptions
{
    public const string SectionName = "LocalDevAuth";

    /// <summary>
    /// Must be false in Production. Only set true in Development via appsettings or env.
    /// </summary>
    public bool Enabled { get; set; }

    public string Issuer { get; set; } = "https://localhost/emp-dev-auth";

    public string Audience { get; set; } = "emp-platform-local";

    /// <summary>
    /// Symmetric signing key (min 32 characters). Override via LocalDevAuth__SigningKey in Docker.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 480;

    public int RefreshTokenExpirationDays { get; set; } = 7;

    public List<LocalDevAuthUserOptions> Users { get; set; } = [];
}

public sealed class LocalDevAuthUserOptions
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Role { get; set; } = "SystemAdministrator";

    public string TenantId { get; set; } = "local-tenant-001";
}
