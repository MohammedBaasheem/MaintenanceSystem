using MaintenanceSystem.Domain.Entities;

namespace MaintenanceSystem.Application.Abstractions.Security;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}

