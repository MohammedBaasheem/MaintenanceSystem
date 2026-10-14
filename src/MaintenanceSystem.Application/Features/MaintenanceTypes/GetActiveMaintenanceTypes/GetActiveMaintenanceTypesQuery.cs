using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.GetActiveMaintenanceTypes;

public record GetActiveMaintenanceTypesQuery : IRequest<IReadOnlyList<MaintenanceTypeDto>>;

