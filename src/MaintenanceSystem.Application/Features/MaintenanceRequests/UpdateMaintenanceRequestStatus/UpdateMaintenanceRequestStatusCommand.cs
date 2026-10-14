using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MaintenanceSystem.Domain.Enums;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.UpdateMaintenanceRequestStatus;

public record UpdateMaintenanceRequestStatusCommand(Guid RequestId, MaintenanceRequestStatus NewStatus) : IRequest<MaintenanceRequestDto>;

