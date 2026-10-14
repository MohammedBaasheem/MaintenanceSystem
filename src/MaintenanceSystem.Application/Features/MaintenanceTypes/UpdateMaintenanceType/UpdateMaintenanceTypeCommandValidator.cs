using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.UpdateMaintenanceType;

public class UpdateMaintenanceTypeCommandValidator : AbstractValidator<UpdateMaintenanceTypeCommand>
{
    public UpdateMaintenanceTypeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرف نوع الصيانة مطلوب.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم نوع الصيانة مطلوب.")
            .MaximumLength(100).WithMessage("اسم نوع الصيانة لا يمكن أن يتجاوز 100 حرف.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("وصف نوع الصيانة لا يمكن أن يتجاوز 500 حرف.");
    }
}

