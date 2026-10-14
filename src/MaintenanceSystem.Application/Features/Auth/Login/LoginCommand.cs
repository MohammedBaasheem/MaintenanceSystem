using MediatR;

namespace MaintenanceSystem.Application.Features.Auth.Login;

public sealed record LoginCommand(string UserName, string Password) : IRequest<LoginResult>;

