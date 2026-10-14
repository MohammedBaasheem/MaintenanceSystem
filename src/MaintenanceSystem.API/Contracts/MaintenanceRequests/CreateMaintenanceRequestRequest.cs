using MaintenanceSystem.Domain.Enums;

namespace MaintenanceSystem.API.Contracts.MaintenanceRequests;

public record CreateMaintenanceRequestRequest(
    string Title,
    string Description,
    MaintenancePriority Priority,
    Guid MaintenanceTypeId);

