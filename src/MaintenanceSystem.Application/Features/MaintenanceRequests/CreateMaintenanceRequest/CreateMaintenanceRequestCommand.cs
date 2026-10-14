using MaintenanceSystem.Domain.Enums;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.CreateMaintenanceRequest;

public record CreateMaintenanceRequestCommand(
    string Title,
    string Description,
    MaintenancePriority Priority,
    Guid MaintenanceTypeId) : IRequest<CreateMaintenanceRequestResult>;

