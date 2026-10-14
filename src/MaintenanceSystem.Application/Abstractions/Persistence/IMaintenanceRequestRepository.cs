using MaintenanceSystem.Domain.Entities;

namespace MaintenanceSystem.Application.Abstractions.Persistence;

public interface IMaintenanceRequestRepository
{
    Task<MaintenanceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(MaintenanceRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(MaintenanceRequest request, CancellationToken cancellationToken = default);
}

