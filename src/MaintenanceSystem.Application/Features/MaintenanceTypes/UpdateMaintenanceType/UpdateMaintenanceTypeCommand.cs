using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.UpdateMaintenanceType;

public record UpdateMaintenanceTypeCommand(Guid Id, string Name, string? Description) : IRequest<UpdateMaintenanceTypeResult>;

