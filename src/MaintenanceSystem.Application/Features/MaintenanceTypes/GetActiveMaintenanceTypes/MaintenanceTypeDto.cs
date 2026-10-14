namespace MaintenanceSystem.Application.Features.MaintenanceTypes.GetActiveMaintenanceTypes;

public record MaintenanceTypeDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc);

