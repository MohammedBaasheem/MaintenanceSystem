using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Domain.Entities;
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

