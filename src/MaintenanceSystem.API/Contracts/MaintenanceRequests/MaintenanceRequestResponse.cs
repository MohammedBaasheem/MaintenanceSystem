using MaintenanceSystem.Domain.Enums;

namespace MaintenanceSystem.API.Contracts.MaintenanceRequests;

public record MaintenanceRequestResponse(
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

