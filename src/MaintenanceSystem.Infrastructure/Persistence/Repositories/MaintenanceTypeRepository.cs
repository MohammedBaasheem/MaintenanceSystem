using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceSystem.Infrastructure.Persistence.Repositories;

public class MaintenanceTypeRepository : IMaintenanceTypeRepository
{
    private readonly AppDbContext _context;

    public MaintenanceTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceTypes
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MaintenanceType>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MaintenanceTypes
            .AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var trimmedName = name.Trim();
        return await _context.MaintenanceTypes
            .AnyAsync(m => m.Name == trimmedName, cancellationToken);
    }

    public async Task<bool> ExistsByNameExceptIdAsync(string name, Guid id, CancellationToken cancellationToken = default)
    {
        var trimmedName = name.Trim();
        return await _context.MaintenanceTypes
            .AnyAsync(m => m.Name == trimmedName && m.Id != id, cancellationToken);
    }

    public async Task AddAsync(MaintenanceType maintenanceType, CancellationToken cancellationToken = default)
    {
        await _context.MaintenanceTypes.AddAsync(maintenanceType, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MaintenanceType maintenanceType, CancellationToken cancellationToken = default)
    {
        _context.MaintenanceTypes.Update(maintenanceType);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

