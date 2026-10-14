using FluentValidation;

namespace MaintenanceSystem.Application.Features.MaintenanceRequests.GetAllMaintenanceRequests;

public class GetAllMaintenanceRequestsQueryValidator : AbstractValidator<GetAllMaintenanceRequestsQuery>
{
    public GetAllMaintenanceRequestsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("رقم الصفحة يجب أن يكون 1 أو أكثر.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("حجم الصفحة يجب أن يكون بين 1 و 100.");

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status.HasValue).WithMessage("قيمة الحالة غير صحيحة.");

        RuleFor(x => x.Priority)
            .IsInEnum().When(x => x.Priority.HasValue).WithMessage("قيمة الأولوية غير صحيحة.");
    }
}

