using MaintenanceSystem.Domain.Entities;

namespace MaintenanceSystem.Application.Abstractions.Persistence;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

