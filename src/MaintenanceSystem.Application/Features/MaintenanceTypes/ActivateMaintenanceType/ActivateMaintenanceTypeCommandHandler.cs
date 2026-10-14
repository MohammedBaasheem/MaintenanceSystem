using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.ActivateMaintenanceType;

public class ActivateMaintenanceTypeCommandHandler : IRequestHandler<ActivateMaintenanceTypeCommand>
{
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;

    public ActivateMaintenanceTypeCommandHandler(IMaintenanceTypeRepository maintenanceTypeRepository)
    {
        _maintenanceTypeRepository = maintenanceTypeRepository;
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
    }
}

