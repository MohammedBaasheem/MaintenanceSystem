using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Domain.Entities;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.CreateMaintenanceType;

public class CreateMaintenanceTypeCommandHandler : IRequestHandler<CreateMaintenanceTypeCommand, CreateMaintenanceTypeResult>
{
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;

    public CreateMaintenanceTypeCommandHandler(IMaintenanceTypeRepository maintenanceTypeRepository)
    {
        _maintenanceTypeRepository = maintenanceTypeRepository;
    }

    public async Task<CreateMaintenanceTypeResult> Handle(CreateMaintenanceTypeCommand request, CancellationToken cancellationToken)
    {
        if (await _maintenanceTypeRepository.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new ConflictException($"نوع الصيانة '{request.Name.Trim()}' مسجل مسبقاً.");
        }

        var maintenanceType = new MaintenanceType(request.Name, request.Description);

        await _maintenanceTypeRepository.AddAsync(maintenanceType, cancellationToken);

        return new CreateMaintenanceTypeResult(
            maintenanceType.Id,
            maintenanceType.Name,
            maintenanceType.Description,
            maintenanceType.IsActive,
            maintenanceType.CreatedAtUtc);
    }
}

