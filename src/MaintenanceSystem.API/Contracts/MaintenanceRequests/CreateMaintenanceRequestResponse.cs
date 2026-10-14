using MaintenanceSystem.Domain.Enums;

namespace MaintenanceSystem.API.Contracts.MaintenanceRequests;

public record CreateMaintenanceRequestResponse(
    Guid Id,
    string Title,
    string Description,
    MaintenancePriority Priority,
    MaintenanceRequestStatus Status,
    Guid MaintenanceTypeId,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc);

