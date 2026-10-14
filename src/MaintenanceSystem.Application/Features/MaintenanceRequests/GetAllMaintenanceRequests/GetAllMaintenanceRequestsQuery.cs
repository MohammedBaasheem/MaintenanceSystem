using MaintenanceSystem.Application.Common.Models;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MaintenanceSystem.Domain.Enums;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetAllMaintenanceRequests;

public record GetAllMaintenanceRequestsQuery(
    int Page = 1,
    int PageSize = 10,
    MaintenanceRequestStatus? Status = null,
    MaintenancePriority? Priority = null,
    Guid? TechnicianId = null,
    Guid? MaintenanceTypeId = null) : IRequest<PagedResult<MaintenanceRequestDto>>;

