using FluentValidation;

namespace MaintenanceSystem.Application.Features.System.Ping;

public sealed class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
    {
        RuleFor(x => x.Message)
            .MaximumLength(10)
            .WithMessage("طول الرسالة يجب ألا يتجاوز 10 أحرف.");
    }
}

