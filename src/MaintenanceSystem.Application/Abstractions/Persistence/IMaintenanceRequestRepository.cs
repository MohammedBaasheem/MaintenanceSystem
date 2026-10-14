using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Enums;

namespace MaintenanceSystem.Application.Abstractions.Persistence;

public interface IMaintenanceRequestRepository
{
    Task<MaintenanceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<MaintenanceRequest> Items, int TotalCount)> GetMyRequestsAsync(
        Guid userId,
        int page,
        int pageSize,
        MaintenanceRequestStatus? status,
        MaintenancePriority? priority,
        CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<MaintenanceRequest> Items, int TotalCount)> GetAssignedToTechnicianAsync(
        Guid technicianId,
        int page,
        int pageSize,
        MaintenanceRequestStatus? status,
        CancellationToken cancellationToken = default);
    Task AddAsync(MaintenanceRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(MaintenanceRequest request, CancellationToken cancellationToken = default);
}

