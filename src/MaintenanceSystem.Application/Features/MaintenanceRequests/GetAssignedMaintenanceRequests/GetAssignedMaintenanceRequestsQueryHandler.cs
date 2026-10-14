using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Application.Common.Models;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetAssignedMaintenanceRequests;

public class GetAssignedMaintenanceRequestsQueryHandler : IRequestHandler<GetAssignedMaintenanceRequestsQuery, PagedResult<MaintenanceRequestDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMaintenanceRequestRepository _maintenanceRequestRepository;

    public GetAssignedMaintenanceRequestsQueryHandler(
        ICurrentUserService currentUserService,
        IMaintenanceRequestRepository maintenanceRequestRepository)
    {
        _currentUserService = currentUserService;
        _maintenanceRequestRepository = maintenanceRequestRepository;
    }

    public async Task<PagedResult<MaintenanceRequestDto>> Handle(GetAssignedMaintenanceRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("المستخدم غير مصادق عليه.");

        var (items, totalCount) = await _maintenanceRequestRepository.GetAssignedToTechnicianAsync(
            userId,
            request.Page,
            request.PageSize,
            request.Status,
            cancellationToken);

        var dtos = items.Select(r => new MaintenanceRequestDto(
            r.Id,
            r.Title,
            r.Description,
            r.Priority,
            r.Status,
            r.MaintenanceTypeId,
            r.CreatedByUserId,
            r.AssignedTechnicianId,
            r.RejectionReason,
            r.AssignedAtUtc,
            r.ResolvedAtUtc,
            r.ClosedAtUtc,
            r.UpdatedAtUtc,
            r.CreatedAtUtc)).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResult<MaintenanceRequestDto>(
            dtos,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}

