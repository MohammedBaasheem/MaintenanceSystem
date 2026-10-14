using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.ActivateMaintenanceType;

public record ActivateMaintenanceTypeCommand(Guid Id) : IRequest;

