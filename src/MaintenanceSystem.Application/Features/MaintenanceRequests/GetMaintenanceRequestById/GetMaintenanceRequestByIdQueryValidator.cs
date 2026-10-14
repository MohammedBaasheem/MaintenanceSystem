using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetMaintenanceRequestById;

public class GetMaintenanceRequestByIdQueryValidator : AbstractValidator<GetMaintenanceRequestByIdQuery>
{
    public GetMaintenanceRequestByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("معرّف الطلب مطلوب.");
    }
}

