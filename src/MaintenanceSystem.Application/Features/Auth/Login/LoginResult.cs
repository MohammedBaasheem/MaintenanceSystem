namespace MaintenanceSystem.Application.Features.Auth.Login;

public sealed record LoginResult(string Token, DateTime ExpiresAtUtc);

