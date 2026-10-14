using MaintenanceSystem.Application.Common.Models;
using MaintenanceSystem.Domain.Enums;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;

public record GetMyMaintenanceRequestsQuery(
    int Page = 1,
    int PageSize = 10,
    MaintenanceRequestStatus? Status = null,
    MaintenancePriority? Priority = null) : IRequest<PagedResult<MaintenanceRequestDto>>;

