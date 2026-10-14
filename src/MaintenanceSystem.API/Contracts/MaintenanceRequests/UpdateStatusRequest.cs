using MaintenanceSystem.Domain.Enums;

namespace MaintenanceSystem.API.Contracts.MaintenanceRequests;

public record UpdateStatusRequest(MaintenanceRequestStatus NewStatus);

