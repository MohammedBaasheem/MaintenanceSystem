using MaintenanceSystem.API.Authorization;
using MaintenanceSystem.API.Contracts.MaintenanceTypes;
using MaintenanceSystem.Application.Features.MaintenanceTypes.ActivateMaintenanceType;
using MaintenanceSystem.Application.Features.MaintenanceTypes.CreateMaintenanceType;
using MaintenanceSystem.Application.Features.MaintenanceTypes.DeactivateMaintenanceType;
using MaintenanceSystem.Application.Features.MaintenanceTypes.GetActiveMaintenanceTypes;
using MaintenanceSystem.Application.Features.MaintenanceTypes.UpdateMaintenanceType;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceSystem.API.Controllers;

[ApiController]
[Route("api/maintenance-types")]
[Authorize]
public class MaintenanceTypesController : ControllerBase
{
    private readonly ISender _sender;

    public MaintenanceTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetActiveMaintenanceTypesQuery(), cancellationToken);

        var response = result.Select(t => new MaintenanceTypeResponse(
            t.Id,
            t.Name,
            t.Description,
            t.IsActive,
            t.CreatedAtUtc));

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = Policies.MaintenanceAdminOnly)]
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
    [Authorize(Policy = Policies.MaintenanceAdminOnly)]
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
    [Authorize(Policy = Policies.MaintenanceAdminOnly)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ActivateMaintenanceTypeCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.MaintenanceAdminOnly)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateMaintenanceTypeCommand(id), cancellationToken);
        return NoContent();
    }
}

