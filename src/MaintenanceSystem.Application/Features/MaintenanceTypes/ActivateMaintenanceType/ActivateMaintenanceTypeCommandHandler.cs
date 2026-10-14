using MaintenanceSystem.Application.Abstractions.Caching;
using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.ActivateMaintenanceType;

public class ActivateMaintenanceTypeCommandHandler : IRequestHandler<ActivateMaintenanceTypeCommand>
{
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;
    private readonly IMaintenanceTypeCache _maintenanceTypeCache;
    private readonly ILogger<ActivateMaintenanceTypeCommandHandler> _logger;

    public ActivateMaintenanceTypeCommandHandler(
        IMaintenanceTypeRepository maintenanceTypeRepository,
        IMaintenanceTypeCache maintenanceTypeCache,
        ILogger<ActivateMaintenanceTypeCommandHandler> logger)
    {
        _maintenanceTypeRepository = maintenanceTypeRepository;
        _maintenanceTypeCache = maintenanceTypeCache;
        _logger = logger;
    }

    public async Task Handle(ActivateMaintenanceTypeCommand request, CancellationToken cancellationToken)
    {
        var maintenanceType = await _maintenanceTypeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (maintenanceType == null)
        {
            throw new NotFoundException($"نوع الصيانة بالمعرف '{request.Id}' غير موجود.");
        }

        maintenanceType.Activate();

        await _maintenanceTypeRepository.UpdateAsync(maintenanceType, cancellationToken);

        try
        {
            await _maintenanceTypeCache.InvalidateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate maintenance types cache.");
        }
    }
}

