using FluentValidation;

namespace MaintenanceSystem.Application.Features.System.Ping;

public sealed class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
    {
    }
}

