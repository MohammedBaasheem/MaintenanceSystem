using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.DeactivateMaintenanceType;

public class DeactivateMaintenanceTypeCommandHandler : IRequestHandler<DeactivateMaintenanceTypeCommand>
{
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;

    public DeactivateMaintenanceTypeCommandHandler(IMaintenanceTypeRepository maintenanceTypeRepository)
    {
        _maintenanceTypeRepository = maintenanceTypeRepository;
    }

    public async Task Handle(DeactivateMaintenanceTypeCommand request, CancellationToken cancellationToken)
    {
        var maintenanceType = await _maintenanceTypeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (maintenanceType == null)
        {
            throw new NotFoundException($"نوع الصيانة بالمعرف '{request.Id}' غير موجود.");
        }

        maintenanceType.Deactivate();

        await _maintenanceTypeRepository.UpdateAsync(maintenanceType, cancellationToken);
    }
}

