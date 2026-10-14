using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.RejectMaintenanceRequest;

public class RejectMaintenanceRequestCommandHandler : IRequestHandler<RejectMaintenanceRequestCommand, MaintenanceRequestDto>
{
    private readonly IMaintenanceRequestRepository _requestRepository;

    public RejectMaintenanceRequestCommandHandler(IMaintenanceRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<MaintenanceRequestDto> Handle(RejectMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _requestRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (maintenanceRequest == null)
        {
            throw new NotFoundException($"طلب الصيانة بالمعرّف '{request.RequestId}' غير موجود.");
        }

        maintenanceRequest.Reject(request.Reason);

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

