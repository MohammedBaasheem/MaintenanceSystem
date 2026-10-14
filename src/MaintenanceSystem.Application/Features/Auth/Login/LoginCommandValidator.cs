using FluentValidation;

namespace MaintenanceSystem.Application.Features.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.")
            .MaximumLength(256).WithMessage("البريد الإلكتروني لا يجب أن يتجاوز 256 حرفاً.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
            .MinimumLength(8).WithMessage("كلمة المرور يجب ألا تقل عن 8 أحرف.")
            .MaximumLength(128).WithMessage("كلمة المرور لا يجب أن تتجاوز 128 حرفاً.");
    }
}

