using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Models;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetAllMaintenanceRequests;

public class GetAllMaintenanceRequestsQueryHandler : IRequestHandler<GetAllMaintenanceRequestsQuery, PagedResult<MaintenanceRequestDto>>
{
    private readonly IMaintenanceRequestRepository _maintenanceRequestRepository;

    public GetAllMaintenanceRequestsQueryHandler(IMaintenanceRequestRepository maintenanceRequestRepository)
    {
        _maintenanceRequestRepository = maintenanceRequestRepository;
    }

    public async Task<PagedResult<MaintenanceRequestDto>> Handle(GetAllMaintenanceRequestsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _maintenanceRequestRepository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Status,
            request.Priority,
            request.TechnicianId,
            request.MaintenanceTypeId,
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

