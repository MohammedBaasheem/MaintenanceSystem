using MaintenanceSystem.API.Contracts.System;
using MaintenanceSystem.Application.Features.System.Ping;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    private readonly ISender _sender;

    public PingController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Ping([FromQuery] string? message, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PingQuery(message), cancellationToken);
        var response = new PingResponse(result.Message, result.TimestampUtc);
        return Ok(response);
    }
}

