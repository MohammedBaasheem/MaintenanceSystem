using MaintenanceSystem.Domain.Common;
using MaintenanceSystem.Domain.Exceptions;

namespace MaintenanceSystem.Domain.Entities;

public class MaintenanceType : Entity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private MaintenanceType() { } // Required for EF Core

    public MaintenanceType(string name, string? description = null)
    {
        ValidateName(name);
        ValidateDescription(description);

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive = true;
        UpdatedAtUtc = null;
    }

    public void Rename(string name, DateTime? utcNow = null)
    {
        ValidateName(name);

        var trimmedName = name.Trim();
        if (Name == trimmedName)
            return;

        Name = trimmedName;
        UpdatedAtUtc = utcNow ?? DateTime.UtcNow;
    }

    public void UpdateDescription(string? description, DateTime? utcNow = null)
    {
        ValidateDescription(description);

        var trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (Description == trimmedDescription)
            return;

        Description = trimmedDescription;
        UpdatedAtUtc = utcNow ?? DateTime.UtcNow;
    }

    public void Activate(DateTime? utcNow = null)
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAtUtc = utcNow ?? DateTime.UtcNow;
    }

    public void Deactivate(DateTime? utcNow = null)
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAtUtc = utcNow ?? DateTime.UtcNow;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("Maintenance type name cannot be empty.");

        if (name.Trim().Length > 100)
            throw new BusinessRuleViolationException("Maintenance type name cannot exceed 100 characters.");
    }

    private static void ValidateDescription(string? description)
    {
        if (description != null && description.Trim().Length > 500)
            throw new BusinessRuleViolationException("Maintenance type description cannot exceed 500 characters.");
    }
}

