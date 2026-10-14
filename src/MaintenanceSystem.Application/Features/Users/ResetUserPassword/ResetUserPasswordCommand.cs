using MediatR;

namespace MaintenanceSystem.Application.Features.Users.ResetUserPassword;

public record ResetUserPasswordCommand(Guid UserId) : IRequest<ResetUserPasswordResponse>;

