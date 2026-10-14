namespace MaintenanceSystem.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "MaintenanceSystem";
    public string Audience { get; set; } = "MaintenanceSystemClient";
    public string SigningKey { get; set; } = string.Empty;
    public int LifetimeMinutes { get; set; } = 60;
}

