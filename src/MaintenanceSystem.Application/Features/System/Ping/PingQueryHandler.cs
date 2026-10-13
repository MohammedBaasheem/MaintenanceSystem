using MediatR;

namespace MaintenanceSystem.Application.Features.System.Ping;

public sealed class PingQueryHandler : IRequestHandler<PingQuery, PingResponse>
{
    public Task<PingResponse> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        var response = new PingResponse("Pong", DateTime.UtcNow);
        return Task.FromResult(response);
    }
}

