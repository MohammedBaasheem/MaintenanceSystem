using MaintenanceSystem.Domain.Enums;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.CreateMaintenanceRequest;

public record CreateMaintenanceRequestResult(
    Guid Id,
    string Title,
    string Description,
    MaintenancePriority Priority,
    MaintenanceRequestStatus Status,
    Guid MaintenanceTypeId,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc);

