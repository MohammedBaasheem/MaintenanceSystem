using MediatR;

namespace MaintenanceSystem.Application.Features.System.Ping;

public sealed record PingQuery(string? Message = null) : IRequest<PingResult>;

