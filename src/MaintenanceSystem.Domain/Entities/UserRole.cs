using MaintenanceSystem.Domain.Exceptions;

namespace MaintenanceSystem.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    private UserRole() { } // Required for EF Core

    public UserRole(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty)
            throw new BusinessRuleViolationException("UserId cannot be empty.");

        if (roleId == Guid.Empty)
            throw new BusinessRuleViolationException("RoleId cannot be empty.");

        UserId = userId;
        RoleId = roleId;
    }
}

