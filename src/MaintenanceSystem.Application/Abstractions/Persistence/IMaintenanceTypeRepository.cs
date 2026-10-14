using MaintenanceSystem.Domain.Entities;

namespace MaintenanceSystem.Application.Abstractions.Persistence;

public interface IMaintenanceTypeRepository
{
    Task<MaintenanceType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceType>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(MaintenanceType maintenanceType, CancellationToken cancellationToken = default);
    Task UpdateAsync(MaintenanceType maintenanceType, CancellationToken cancellationToken = default);
}

