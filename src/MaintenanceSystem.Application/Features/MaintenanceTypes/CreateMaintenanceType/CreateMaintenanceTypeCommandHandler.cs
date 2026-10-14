using MaintenanceSystem.Application.Abstractions.Caching;
using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.CreateMaintenanceType;

public class CreateMaintenanceTypeCommandHandler : IRequestHandler<CreateMaintenanceTypeCommand, CreateMaintenanceTypeResult>
{
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;
    private readonly IMaintenanceTypeCache _maintenanceTypeCache;
    private readonly ILogger<CreateMaintenanceTypeCommandHandler> _logger;

    public CreateMaintenanceTypeCommandHandler(
        IMaintenanceTypeRepository maintenanceTypeRepository,
        IMaintenanceTypeCache maintenanceTypeCache,
        ILogger<CreateMaintenanceTypeCommandHandler> logger)
    {
        _maintenanceTypeRepository = maintenanceTypeRepository;
        _maintenanceTypeCache = maintenanceTypeCache;
        _logger = logger;
    }

    public async Task<CreateMaintenanceTypeResult> Handle(CreateMaintenanceTypeCommand request, CancellationToken cancellationToken)
    {
        if (await _maintenanceTypeRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new ConflictException($"نوع الصيانة '{request.Name.Trim()}' مسجل مسبقاً.");
        }

        var maintenanceType = new MaintenanceType(request.Name, request.Description);

        await _maintenanceTypeRepository.AddAsync(maintenanceType, cancellationToken);

        try
        {
            await _maintenanceTypeCache.InvalidateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate maintenance types cache.");
        }

        return new CreateMaintenanceTypeResult(
            maintenanceType.Id,
            maintenanceType.Name,
            maintenanceType.Description,
            maintenanceType.IsActive,
            maintenanceType.CreatedAtUtc);
    }
}

