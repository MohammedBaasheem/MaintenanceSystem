using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.DeactivateMaintenanceType;

public class DeactivateMaintenanceTypeCommandValidator : AbstractValidator<DeactivateMaintenanceTypeCommand>
{
    public DeactivateMaintenanceTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف نوع الصيانة مطلوب.");
    }
}

