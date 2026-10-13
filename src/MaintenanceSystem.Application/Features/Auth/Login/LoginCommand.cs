using MediatR;

namespace MaintenanceSystem.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

