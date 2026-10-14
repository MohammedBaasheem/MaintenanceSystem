using MediatR;

namespace MaintenanceSystem.Application.Features.Users.UpdateUserRole;

public record UpdateUserRoleCommand(Guid UserId, string Role) : IRequest;

