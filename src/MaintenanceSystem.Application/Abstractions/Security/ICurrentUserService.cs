namespace MaintenanceSystem.Application.Abstractions.Security;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}

