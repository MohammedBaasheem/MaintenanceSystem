using System.Security.Claims;
using MaintenanceSystem.API.Contracts.Auth;
using MaintenanceSystem.Application.Features.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Diagnostic/testing endpoint to verify token validity and SecurityStamp revocation.
    /// Note: This is an internal diagnostic endpoint for verification, not an official ADR-0001 public contract.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        var userName = User.Identity?.Name ?? User.FindFirst("unique_name")?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        return Ok(new { userId, userName, role });
    }
}

