using FluentValidation;

namespace MaintenanceSystem.Application.Features.Users.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
            .MaximumLength(50).WithMessage("اسم المستخدم لا يمكن أن يتجاوز 50 حرفاً.")
            .Matches(@"^[a-zA-Z0-9_\-\.]+$").WithMessage("اسم المستخدم يحتوي على أحرف غير مسموح بها.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .MaximumLength(256).WithMessage("البريد الإلكتروني لا يمكن أن يتجاوز 256 حرفاً.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("الدور مطلوب.")
            .MaximumLength(50).WithMessage("الدور لا يمكن أن يتجاوز 50 حرفاً.");
    }
}

