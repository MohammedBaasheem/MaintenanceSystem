using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.UpdateMaintenanceType;

public class UpdateMaintenanceTypeCommandHandler : IRequestHandler<UpdateMaintenanceTypeCommand, UpdateMaintenanceTypeResult>
{
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;

    public UpdateMaintenanceTypeCommandHandler(IMaintenanceTypeRepository maintenanceTypeRepository)
    {
        _maintenanceTypeRepository = maintenanceTypeRepository;
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

        return new UpdateMaintenanceTypeResult(
            maintenanceType.Id,
            maintenanceType.Name,
            maintenanceType.Description,
            maintenanceType.IsActive,
            maintenanceType.UpdatedAtUtc);
    }
}

