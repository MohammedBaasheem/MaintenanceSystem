using MaintenanceSystem.Application.Abstractions.Persistence;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.GetActiveMaintenanceTypes;

public class GetActiveMaintenanceTypesQueryHandler : IRequestHandler<GetActiveMaintenanceTypesQuery, IReadOnlyList<MaintenanceTypeDto>>
{
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;

    public GetActiveMaintenanceTypesQueryHandler(IMaintenanceTypeRepository maintenanceTypeRepository)
    {
        _maintenanceTypeRepository = maintenanceTypeRepository;
    }

    public async Task<IReadOnlyList<MaintenanceTypeDto>> Handle(GetActiveMaintenanceTypesQuery request, CancellationToken cancellationToken)
    {
        var activeTypes = await _maintenanceTypeRepository.GetActiveAsync(cancellationToken);

        return activeTypes.Select(t => new MaintenanceTypeDto(
            t.Id,
            t.Name,
            t.Description,
            t.IsActive,
            t.CreatedAtUtc)).ToList();
    }
}

