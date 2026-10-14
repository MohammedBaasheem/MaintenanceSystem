using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.AssignTechnician;

public class AssignTechnicianCommandValidator : AbstractValidator<AssignTechnicianCommand>
{
    public AssignTechnicianCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("RequestId cannot be empty.");

        RuleFor(x => x.TechnicianId)
            .NotEmpty().WithMessage("TechnicianId cannot be empty.");
    }
}

