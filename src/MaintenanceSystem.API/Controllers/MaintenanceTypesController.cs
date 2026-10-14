using MaintenanceSystem.API.Authorization;
using MaintenanceSystem.API.Contracts.MaintenanceTypes;
using MaintenanceSystem.Application.Features.MaintenanceTypes.CreateMaintenanceType;
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
}

