using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MaintenanceSystem.Domain.Enums;
using MaintenanceSystem.Domain.Exceptions;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.UpdateMaintenanceRequestStatus;

public class UpdateMaintenanceRequestStatusCommandHandler : IRequestHandler<UpdateMaintenanceRequestStatusCommand, MaintenanceRequestDto>
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMaintenanceRequestStatusCommandHandler(
        IMaintenanceRequestRepository requestRepository,
        ICurrentUserService currentUserService)
    {
        _requestRepository = requestRepository;
        _currentUserService = currentUserService;
    }

    public async Task<MaintenanceRequestDto> Handle(UpdateMaintenanceRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedException("المستخدم غير مصادق عليه.");

        var maintenanceRequest = await _requestRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (maintenanceRequest == null)
        {
            throw new NotFoundException($"طلب الصيانة بالمعرّف '{request.RequestId}' غير موجود.");
        }

        var isAdmin = string.Equals(_currentUserService.Role, RoleNames.MaintenanceAdmin, StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && maintenanceRequest.AssignedTechnicianId != currentUserId)
        {
            throw new ForbiddenException("غير مصرح لك بتحديث حالة هذا الطلب.");
        }

        switch (request.NewStatus)
        {
            case MaintenanceRequestStatus.InProgress:
                maintenanceRequest.Start();
                break;
            case MaintenanceRequestStatus.Resolved:
                maintenanceRequest.Resolve();
                break;
            case MaintenanceRequestStatus.Closed:
                maintenanceRequest.Close();
                break;
            default:
                throw new InvalidStateTransitionException(
                    $"الانتقال إلى الحالة '{request.NewStatus}' غير مسموح به عبر هذا المسار.");
        }

        await _requestRepository.UpdateAsync(maintenanceRequest, cancellationToken);

        return new MaintenanceRequestDto(
            maintenanceRequest.Id,
            maintenanceRequest.Title,
            maintenanceRequest.Description,
            maintenanceRequest.Priority,
            maintenanceRequest.Status,
            maintenanceRequest.MaintenanceTypeId,
            maintenanceRequest.CreatedByUserId,
            maintenanceRequest.AssignedTechnicianId,
            maintenanceRequest.RejectionReason,
            maintenanceRequest.AssignedAtUtc,
            maintenanceRequest.ResolvedAtUtc,
            maintenanceRequest.ClosedAtUtc,
            maintenanceRequest.UpdatedAtUtc,
            maintenanceRequest.CreatedAtUtc);
    }
}

