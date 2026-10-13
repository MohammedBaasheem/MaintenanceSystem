namespace MaintenanceSystem.API.Contracts.Auth;

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc);

