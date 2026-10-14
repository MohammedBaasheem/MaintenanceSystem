using MediatR;

namespace MaintenanceSystem.Application.Features.System.Ping;

public sealed class PingQueryHandler : IRequestHandler<PingQuery, PingResult>
{
    public Task<PingResult> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        var responseMessage = string.IsNullOrWhiteSpace(request.Message) ? "Pong" : $"Pong: {request.Message}";
        var result = new PingResult(responseMessage, DateTime.UtcNow);
        return Task.FromResult(result);
    }
}

