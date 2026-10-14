using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.RejectMaintenanceRequest;

public record RejectMaintenanceRequestCommand(Guid RequestId, string Reason) : IRequest<MaintenanceRequestDto>;

