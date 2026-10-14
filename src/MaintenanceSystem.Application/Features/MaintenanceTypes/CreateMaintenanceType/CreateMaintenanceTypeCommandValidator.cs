using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceTypes.CreateMaintenanceType;

public class CreateMaintenanceTypeCommandValidator : AbstractValidator<CreateMaintenanceTypeCommand>
{
    public CreateMaintenanceTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم نوع الصيانة مطلوب.")
            .MaximumLength(100).WithMessage("اسم نوع الصيانة لا يمكن أن يتجاوز 100 حرف.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("وصف نوع الصيانة لا يمكن أن يتجاوز 500 حرف.");
    }
}

