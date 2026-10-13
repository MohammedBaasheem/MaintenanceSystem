using MediatR;

namespace MaintenanceSystem.Application.Features.System.Ping;

public sealed class PingQueryHandler : IRequestHandler<PingQuery, PingResult>
{
    public Task<PingResult> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        var result = new PingResult("Pong", DateTime.UtcNow);
        return Task.FromResult(result);
    }
}

