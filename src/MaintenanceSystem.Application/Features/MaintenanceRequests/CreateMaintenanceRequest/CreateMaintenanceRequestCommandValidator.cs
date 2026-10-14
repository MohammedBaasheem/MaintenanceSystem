using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.CreateMaintenanceRequest;

public class CreateMaintenanceRequestCommandValidator : AbstractValidator<CreateMaintenanceRequestCommand>
{
    public CreateMaintenanceRequestCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("العنوان مطلوب.")
            .MaximumLength(200).WithMessage("العنوان لا يمكن أن يتجاوز 200 حرف.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("الوصف مطلوب.")
            .MaximumLength(2000).WithMessage("الوصف لا يمكن أن يتجاوز 2000 حرف.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("أولوية الطلب غير صحيحة.");

        RuleFor(x => x.MaintenanceTypeId)
            .NotEmpty().WithMessage("معرّف نوع الصيانة مطلوب.");
    }
}

