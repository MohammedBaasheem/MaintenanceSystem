namespace MaintenanceSystem.Application.Features.MaintenanceTypes.UpdateMaintenanceType;

public record UpdateMaintenanceTypeResult(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime? UpdatedAtUtc);

