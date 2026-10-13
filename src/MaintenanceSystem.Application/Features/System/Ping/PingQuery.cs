using MediatR;

namespace MaintenanceSystem.Application.Features.System.Ping;

public sealed record PingQuery : IRequest<PingResult>;

