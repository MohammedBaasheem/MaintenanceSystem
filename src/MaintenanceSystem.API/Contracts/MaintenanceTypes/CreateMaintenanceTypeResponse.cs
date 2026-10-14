namespace MaintenanceSystem.API.Contracts.MaintenanceTypes;

public record CreateMaintenanceTypeResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc);

