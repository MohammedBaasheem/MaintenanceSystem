using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceSystem.Infrastructure.Persistence.Repositories;

public class MaintenanceRequestRepository : IMaintenanceRequestRepository
{
    private readonly AppDbContext _context;

    public MaintenanceRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<MaintenanceRequest> Items, int TotalCount)> GetMyRequestsAsync(
        Guid userId,
        int page,
        int pageSize,
        MaintenanceRequestStatus? status,
        MaintenancePriority? priority,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MaintenanceRequests
            .AsNoTracking()
            .Where(r => r.CreatedByUserId == userId);

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(r => r.Priority == priority.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(MaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        await _context.MaintenanceRequests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        _context.MaintenanceRequests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

