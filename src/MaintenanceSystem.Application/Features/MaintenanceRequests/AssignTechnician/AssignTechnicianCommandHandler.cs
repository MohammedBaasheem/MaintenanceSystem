using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Enums;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.AssignTechnician;

public class AssignTechnicianCommandHandler : IRequestHandler<AssignTechnicianCommand, MaintenanceRequestDto>
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;

    public AssignTechnicianCommandHandler(
        IMaintenanceRequestRepository requestRepository,
        IUserRepository userRepository)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
    }

    public async Task<MaintenanceRequestDto> Handle(AssignTechnicianCommand request, CancellationToken cancellationToken)
    {
        var maintenanceRequest = await _requestRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (maintenanceRequest == null)
        {
            throw new NotFoundException($"طلب الصيانة بالمعرّف '{request.RequestId}' غير موجود.");
        }

        var technician = await _userRepository.GetByIdAsync(request.TechnicianId, cancellationToken);
        if (technician == null)
        {
            throw new NotFoundException($"المستخدم بالمعرّف '{request.TechnicianId}' غير موجود.");
        }

        var isTechnician = technician.UserRoles.Any(ur => ur.Role?.Name == RoleNames.Technician);
        if (!isTechnician)
        {
            throw new ConflictException($"المستخدم بالمعرّف '{request.TechnicianId}' لا يحمل دور الفني ({RoleNames.Technician}).");
        }

        maintenanceRequest.AssignTechnician(request.TechnicianId);

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

