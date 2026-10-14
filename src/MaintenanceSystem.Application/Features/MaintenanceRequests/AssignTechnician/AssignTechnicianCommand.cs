using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.AssignTechnician;

public record AssignTechnicianCommand(Guid RequestId, Guid TechnicianId) : IRequest<MaintenanceRequestDto>;

