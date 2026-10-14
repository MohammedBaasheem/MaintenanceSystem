using MaintenanceSystem.API.Authorization;
using MaintenanceSystem.API.Contracts.Users;
using MaintenanceSystem.Application.Features.Users.CreateUser;
using MaintenanceSystem.Application.Features.Users.ResetUserPassword;
using MaintenanceSystem.Application.Features.Users.UpdateUserRole;
using MaintenanceSystem.Application.Features.Users.UpdateUserStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.MaintenanceAdminOnly)]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateUserCommand(request.UserName, request.Email, request.Role),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new Contracts.Users.CreateUserResponse(
            result.Id,
            result.UserName,
            result.Email,
            result.Role,
            result.TemporaryPassword));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateUserStatusCommand(id, request.IsActive), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ResetUserPasswordCommand(id), cancellationToken);
        return Ok(new Contracts.Users.ResetUserPasswordResponse(result.TemporaryPassword));
    }

    [HttpPatch("{id:guid}/roles")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateUserRoleCommand(id, request.Role), cancellationToken);
        return NoContent();
    }
}

