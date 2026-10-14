using FluentValidation;

namespace MaintenanceSystem.Application.Features.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب.")
            .MaximumLength(50).WithMessage("اسم المستخدم لا يجب أن يتجاوز 50 حرفاً.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
            .MaximumLength(128).WithMessage("كلمة المرور لا يجب أن تتجاوز 128 حرفاً.");
    }
}

