using MediatR;

namespace MaintenanceSystem.Application.Features.Users.UpdateUserStatus;

public record UpdateUserStatusCommand(Guid UserId, bool IsActive) : IRequest;

