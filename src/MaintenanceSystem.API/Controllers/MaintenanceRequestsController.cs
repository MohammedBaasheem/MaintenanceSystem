using MaintenanceSystem.API.Authorization;
using MaintenanceSystem.API.Contracts.Common;
using MaintenanceSystem.API.Contracts.MaintenanceRequests;
using MaintenanceSystem.Application.Features.MaintenanceRequests.AssignTechnician;
using MaintenanceSystem.Application.Features.MaintenanceRequests.CreateMaintenanceRequest;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetAssignedMaintenanceRequests;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMaintenanceRequestById;
using MaintenanceSystem.Application.Features.MaintenanceRequests.GetMyMaintenanceRequests;
using MaintenanceSystem.Application.Features.MaintenanceRequests.UpdateMaintenanceRequestStatus;
using MaintenanceSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceSystem.API.Controllers;

[ApiController]
[Route("api/maintenance-requests")]
[Authorize]
public class MaintenanceRequestsController : ControllerBase
{
    private readonly ISender _sender;

    public MaintenanceRequestsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateMaintenanceRequestCommand(
                request.Title,
                request.Description,
                request.Priority,
                request.MaintenanceTypeId),
            cancellationToken);

        var response = new CreateMaintenanceRequestResponse(
            result.Id,
            result.Title,
            result.Description,
            result.Priority,
            result.Status,
            result.MaintenanceTypeId,
            result.CreatedByUserId,
            result.CreatedAtUtc);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] MaintenanceRequestStatus? status = null,
        [FromQuery] MaintenancePriority? priority = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetMyMaintenanceRequestsQuery(page, pageSize, status, priority),
            cancellationToken);

        var items = result.Items.Select(r => new MaintenanceRequestResponse(
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

        var response = new PagedResponse<MaintenanceRequestResponse>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage);

        return Ok(response);
    }

    [HttpGet("assigned")]
    [Authorize(Policy = Policies.TechnicianOrAdmin)]
    public async Task<IActionResult> GetAssigned(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] MaintenanceRequestStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetAssignedMaintenanceRequestsQuery(page, pageSize, status),
            cancellationToken);

        var items = result.Items.Select(r => new MaintenanceRequestResponse(
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

        var response = new PagedResponse<MaintenanceRequestResponse>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMaintenanceRequestByIdQuery(id), cancellationToken);

        var response = new MaintenanceRequestResponse(
            result.Id,
            result.Title,
            result.Description,
            result.Priority,
            result.Status,
            result.MaintenanceTypeId,
            result.CreatedByUserId,
            result.AssignedTechnicianId,
            result.RejectionReason,
            result.AssignedAtUtc,
            result.ResolvedAtUtc,
            result.ClosedAtUtc,
            result.UpdatedAtUtc,
            result.CreatedAtUtc);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Policy = Policies.MaintenanceAdminOnly)]
    public async Task<IActionResult> AssignTechnician(
        Guid id,
        [FromBody] AssignTechnicianRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AssignTechnicianCommand(id, request.TechnicianId),
            cancellationToken);

        var response = new MaintenanceRequestResponse(
            result.Id,
            result.Title,
            result.Description,
            result.Priority,
            result.Status,
            result.MaintenanceTypeId,
            result.CreatedByUserId,
            result.AssignedTechnicianId,
            result.RejectionReason,
            result.AssignedAtUtc,
            result.ResolvedAtUtc,
            result.ClosedAtUtc,
            result.UpdatedAtUtc,
            result.CreatedAtUtc);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Policies.TechnicianOrAdmin)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateMaintenanceRequestStatusCommand(id, request.NewStatus),
            cancellationToken);

        var response = new MaintenanceRequestResponse(
            result.Id,
            result.Title,
            result.Description,
            result.Priority,
            result.Status,
            result.MaintenanceTypeId,
            result.CreatedByUserId,
            result.AssignedTechnicianId,
            result.RejectionReason,
            result.AssignedAtUtc,
            result.ResolvedAtUtc,
            result.ClosedAtUtc,
            result.UpdatedAtUtc,
            result.CreatedAtUtc);

        return Ok(response);
    }
}


