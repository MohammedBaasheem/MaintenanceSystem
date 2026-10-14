namespace MaintenanceSystem.Application.Features.MaintenanceTypes.CreateMaintenanceType;

public record CreateMaintenanceTypeResult(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc);

