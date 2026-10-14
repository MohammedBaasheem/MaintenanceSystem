using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetMaintenanceRequestById;

public record GetMaintenanceRequestByIdQuery(Guid Id) : IRequest<MaintenanceRequestDetailsDto>;

