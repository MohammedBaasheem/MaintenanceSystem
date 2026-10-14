using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.UpdateMaintenanceRequestStatus;

public class UpdateMaintenanceRequestStatusCommandValidator : AbstractValidator<UpdateMaintenanceRequestStatusCommand>
{
    public UpdateMaintenanceRequestStatusCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("RequestId cannot be empty.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("NewStatus must be a valid status.");
    }
}

