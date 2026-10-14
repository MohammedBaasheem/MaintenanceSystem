using MaintenanceSystem.Domain.Enums;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetMaintenanceRequestById;

public record MaintenanceRequestDetailsDto(
    Guid Id,
    string Title,
    string Description,
    MaintenancePriority Priority,
    MaintenanceRequestStatus Status,
    Guid MaintenanceTypeId,
    Guid CreatedByUserId,
    Guid? AssignedTechnicianId,
    string? RejectionReason,
    DateTime? AssignedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime CreatedAtUtc);

