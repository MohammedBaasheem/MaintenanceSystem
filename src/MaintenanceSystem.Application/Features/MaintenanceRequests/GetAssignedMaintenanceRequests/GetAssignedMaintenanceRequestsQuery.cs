using MaintenanceSystem.Application.Common.Models;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MaintenanceSystem.Domain.Enums;
using MediatR;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetAssignedMaintenanceRequests;

public record GetAssignedMaintenanceRequestsQuery(
    int Page = 1,
    int PageSize = 10,
    MaintenanceRequestStatus? Status = null) : IRequest<PagedResult<MaintenanceRequestDto>>;

