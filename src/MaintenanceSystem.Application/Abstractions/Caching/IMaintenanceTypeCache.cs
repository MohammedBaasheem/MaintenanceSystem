using MaintenanceSystem.Application.Features.MaintenanceTypes.GetActiveMaintenanceTypes;

namespace MaintenanceSystem.Application.Abstractions.Caching;

public interface IMaintenanceTypeCache
{
    Task<IReadOnlyList<MaintenanceTypeDto>?> GetAsync(CancellationToken cancellationToken = default);
    Task SetAsync(IReadOnlyList<MaintenanceTypeDto> types, CancellationToken cancellationToken = default);
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}

