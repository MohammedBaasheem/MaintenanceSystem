using MaintenanceSystem.Application.Abstractions.Caching;
using MaintenanceSystem.Application.Abstractions.Persistence;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.GetActiveMaintenanceTypes;

public class GetActiveMaintenanceTypesQueryHandler : IRequestHandler<GetActiveMaintenanceTypesQuery, IReadOnlyList<MaintenanceTypeDto>>
{
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;
    private readonly IMaintenanceTypeCache _maintenanceTypeCache;

    public GetActiveMaintenanceTypesQueryHandler(
        IMaintenanceTypeRepository maintenanceTypeRepository,
        IMaintenanceTypeCache maintenanceTypeCache)
    {
        _maintenanceTypeRepository = maintenanceTypeRepository;
        _maintenanceTypeCache = maintenanceTypeCache;
    }

    public async Task<IReadOnlyList<MaintenanceTypeDto>> Handle(GetActiveMaintenanceTypesQuery request, CancellationToken cancellationToken)
    {
        var cached = await _maintenanceTypeCache.GetAsync(cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var activeTypes = await _maintenanceTypeRepository.GetActiveAsync(cancellationToken);

        var dtos = activeTypes.Select(t => new MaintenanceTypeDto(
            t.Id,
            t.Name,
            t.Description,
            t.IsActive,
            t.CreatedAtUtc)).ToList();

        await _maintenanceTypeCache.SetAsync(dtos, cancellationToken);

        return dtos;
    }
}

