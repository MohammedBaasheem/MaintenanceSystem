namespace MaintenanceSystem.API.Contracts.MaintenanceTypes;

public record MaintenanceTypeResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAtUtc);

