using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.RejectMaintenanceRequest;

public class RejectMaintenanceRequestCommandValidator : AbstractValidator<RejectMaintenanceRequestCommand>
{
    public RejectMaintenanceRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("RequestId cannot be empty.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason cannot be empty.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}

