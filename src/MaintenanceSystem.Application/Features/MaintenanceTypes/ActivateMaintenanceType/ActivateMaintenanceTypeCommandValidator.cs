using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.ActivateMaintenanceType;

public class ActivateMaintenanceTypeCommandValidator : AbstractValidator<ActivateMaintenanceTypeCommand>
{
    public ActivateMaintenanceTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف نوع الصيانة مطلوب.");
    }
}

