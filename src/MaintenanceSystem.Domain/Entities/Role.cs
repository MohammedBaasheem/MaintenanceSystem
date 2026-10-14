using MaintenanceSystem.Domain.Common;
using MaintenanceSystem.Domain.Exceptions;

namespace MaintenanceSystem.Domain.Entities;

public class Role : Entity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private Role() { } // Required for EF Core

    public Role(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("Role name cannot be empty.");

        if (name.Trim().Length > 50)
            throw new BusinessRuleViolationException("Role name cannot exceed 50 characters.");

        if (description?.Trim().Length > 200)
            throw new BusinessRuleViolationException("Role description cannot exceed 200 characters.");

        Name = name.Trim();
        Description = description?.Trim();
    }

    public void UpdateDescription(string? description)
    {
        if (description?.Trim().Length > 200)
            throw new BusinessRuleViolationException("Role description cannot exceed 200 characters.");

        Description = description?.Trim();
    }
}

