using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Domain.Enums;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetMaintenanceRequestById;

public class GetMaintenanceRequestByIdQueryHandler : IRequestHandler<GetMaintenanceRequestByIdQuery, MaintenanceRequestDetailsDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMaintenanceRequestRepository _maintenanceRequestRepository;

    public GetMaintenanceRequestByIdQueryHandler(
        ICurrentUserService currentUserService,
        IMaintenanceRequestRepository maintenanceRequestRepository)
    {
        _currentUserService = currentUserService;
        _maintenanceRequestRepository = maintenanceRequestRepository;
    }

    public async Task<MaintenanceRequestDetailsDto> Handle(GetMaintenanceRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("المستخدم غير مصادق عليه.");

        var role = _currentUserService.Role;

        var entity = await _maintenanceRequestRepository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            throw new NotFoundException($"طلب الصيانة بالمعرّف '{request.Id}' غير موجود.");
        }

        var isAuthorized = role switch
        {
            RoleNames.MaintenanceAdmin => true,
            RoleNames.Employee => entity.CreatedByUserId == userId,
            RoleNames.Technician => entity.AssignedTechnicianId == userId,
            _ => false
        };

        if (!isAuthorized)
        {
            throw new ForbiddenException("ليس لديك صلاحية لعرض هذا الطلب.");
        }

        return new MaintenanceRequestDetailsDto(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.Priority,
            entity.Status,
            entity.MaintenanceTypeId,
            entity.CreatedByUserId,
            entity.AssignedTechnicianId,
            entity.RejectionReason,
            entity.AssignedAtUtc,
            entity.ResolvedAtUtc,
            entity.ClosedAtUtc,
            entity.UpdatedAtUtc,
            entity.CreatedAtUtc);
    }
}

