using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.DeactivateMaintenanceType;

public record DeactivateMaintenanceTypeCommand(Guid Id) : IRequest;

