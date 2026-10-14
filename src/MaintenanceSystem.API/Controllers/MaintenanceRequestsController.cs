using MaintenanceSystem.API.Contracts.MaintenanceRequests;
using MaintenanceSystem.Application.Features.MaintenanceRequests.CreateMaintenanceRequest;
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
}

