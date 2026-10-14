namespace MaintenanceSystem.API.Contracts.MaintenanceTypes;

public record UpdateMaintenanceTypeResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime? UpdatedAtUtc);

