using MaintenanceSystem.Application.Abstractions.Caching;
using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.UpdateMaintenanceType;

public class UpdateMaintenanceTypeCommandHandler : IRequestHandler<UpdateMaintenanceTypeCommand, UpdateMaintenanceTypeResult>
{
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;
    private readonly IMaintenanceTypeCache _maintenanceTypeCache;
    private readonly ILogger<UpdateMaintenanceTypeCommandHandler> _logger;

    public UpdateMaintenanceTypeCommandHandler(
        IMaintenanceTypeRepository maintenanceTypeRepository,
        IMaintenanceTypeCache maintenanceTypeCache,
        ILogger<UpdateMaintenanceTypeCommandHandler> logger)
    {
        _maintenanceTypeRepository = maintenanceTypeRepository;
        _maintenanceTypeCache = maintenanceTypeCache;
        _logger = logger;
    }

    public async Task<UpdateMaintenanceTypeResult> Handle(UpdateMaintenanceTypeCommand request, CancellationToken cancellationToken)
    {
        var maintenanceType = await _maintenanceTypeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (maintenanceType == null)
        {
            throw new NotFoundException($"نوع الصيانة بالمعرف '{request.Id}' غير موجود.");
        }

        if (await _maintenanceTypeRepository.ExistsByNameExceptIdAsync(request.Name, request.Id, cancellationToken))
        {
            throw new ConflictException($"نوع الصيانة '{request.Name.Trim()}' مسجل مسبقاً.");
        }

        maintenanceType.Rename(request.Name);
        maintenanceType.UpdateDescription(request.Description);

        await _maintenanceTypeRepository.UpdateAsync(maintenanceType, cancellationToken);

        try
        {
            await _maintenanceTypeCache.InvalidateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate maintenance types cache.");
        }

        return new UpdateMaintenanceTypeResult(
            maintenanceType.Id,
            maintenanceType.Name,
            maintenanceType.Description,
            maintenanceType.IsActive,
            maintenanceType.UpdatedAtUtc);
    }
}

