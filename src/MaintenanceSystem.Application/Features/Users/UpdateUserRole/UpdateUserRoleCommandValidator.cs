using FluentValidation;

namespace MaintenanceSystem.Application.Features.Users.UpdateUserRole;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("الدور مطلوب.")
            .MaximumLength(50).WithMessage("الدور لا يمكن أن يتجاوز 50 حرفاً.");
    }
}

