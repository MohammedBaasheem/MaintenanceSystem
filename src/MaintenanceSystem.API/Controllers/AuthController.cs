using MaintenanceSystem.API.Contracts.Auth;
using MaintenanceSystem.Application.Features.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.UserName, request.Password), cancellationToken);
        return Ok(new LoginResponse(result.Token, result.ExpiresAtUtc));
    }
}

