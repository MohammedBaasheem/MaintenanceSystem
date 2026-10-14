using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.CreateMaintenanceType;

public record CreateMaintenanceTypeCommand(string Name, string? Description) : IRequest<CreateMaintenanceTypeResult>;

