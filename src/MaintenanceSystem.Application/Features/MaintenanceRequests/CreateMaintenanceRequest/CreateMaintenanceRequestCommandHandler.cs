using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Domain.Entities;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.CreateMaintenanceRequest;

public class CreateMaintenanceRequestCommandHandler : IRequestHandler<CreateMaintenanceRequestCommand, CreateMaintenanceRequestResult>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMaintenanceTypeRepository _maintenanceTypeRepository;
    private readonly IMaintenanceRequestRepository _maintenanceRequestRepository;

    public CreateMaintenanceRequestCommandHandler(
        ICurrentUserService currentUserService,
        IMaintenanceTypeRepository maintenanceTypeRepository,
        IMaintenanceRequestRepository maintenanceRequestRepository)
    {
        _currentUserService = currentUserService;
        _maintenanceTypeRepository = maintenanceTypeRepository;
        _maintenanceRequestRepository = maintenanceRequestRepository;
    }

    public async Task<CreateMaintenanceRequestResult> Handle(CreateMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("المستخدم غير مصادق عليه.");

        var maintenanceType = await _maintenanceTypeRepository.GetByIdAsync(request.MaintenanceTypeId, cancellationToken);
        if (maintenanceType is null)
        {
            throw new NotFoundException($"نوع الصيانة بالمعرّف '{request.MaintenanceTypeId}' غير موجود.");
        }

        if (!maintenanceType.IsActive)
        {
            throw new ConflictException($"نوع الصيانة '{maintenanceType.Name}' معطّل ولا يمكن استخدامه.");
        }

        var maintenanceRequest = new MaintenanceRequest(
            request.Title,
            request.Description,
            request.Priority,
            request.MaintenanceTypeId,
            userId);

        await _maintenanceRequestRepository.AddAsync(maintenanceRequest, cancellationToken);

        return new CreateMaintenanceRequestResult(
            maintenanceRequest.Id,
            maintenanceRequest.Title,
            maintenanceRequest.Description,
            maintenanceRequest.Priority,
            maintenanceRequest.Status,
            maintenanceRequest.MaintenanceTypeId,
            maintenanceRequest.CreatedByUserId,
            maintenanceRequest.CreatedAtUtc);
    }
}

