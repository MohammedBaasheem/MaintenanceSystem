using MaintenanceSystem.API.Authorization;
using MaintenanceSystem.API.Contracts.MaintenanceTypes;
using MaintenanceSystem.Application.Features.MaintenanceTypes.ActivateMaintenanceType;
using MaintenanceSystem.Application.Features.MaintenanceTypes.CreateMaintenanceType;
using MaintenanceSystem.Application.Features.MaintenanceTypes.DeactivateMaintenanceType;
using MaintenanceSystem.Application.Features.MaintenanceTypes.UpdateMaintenanceType;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceSystem.API.Controllers;

[ApiController]
[Route("api/maintenance-types")]
[Authorize(Policy = Policies.MaintenanceAdminOnly)]
public class MaintenanceTypesController : ControllerBase
{
    private readonly ISender _sender;

    public MaintenanceTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateMaintenanceTypeCommand(request.Name, request.Description),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new CreateMaintenanceTypeResponse(
            result.Id,
            result.Name,
            result.Description,
            result.IsActive,
            result.CreatedAtUtc));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaintenanceTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateMaintenanceTypeCommand(id, request.Name, request.Description),
            cancellationToken);

        return Ok(new UpdateMaintenanceTypeResponse(
            result.Id,
            result.Name,
            result.Description,
            result.IsActive,
            result.UpdatedAtUtc));
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateMaintenanceTypeCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateMaintenanceTypeCommand(id), cancellationToken);
        return NoContent();
    }
}

