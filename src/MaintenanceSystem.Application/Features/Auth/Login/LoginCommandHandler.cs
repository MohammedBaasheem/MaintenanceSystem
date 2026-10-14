using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MediatR;

namespace MaintenanceSystem.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedException("اسم المستخدم أو كلمة المرور غير صحيحة.");
        }

        if (user.IsLockedOut())
        {
            throw new LockedException("الحساب مقفول مؤقتاً لتكرار محاولات الدخول الخاطئة. يرجى المحاولة لاحقاً.");
        }

        var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            user.RegisterFailedAttempt();
            await _userRepository.UpdateAsync(user, cancellationToken);
            throw new UnauthorizedException("اسم المستخدم أو كلمة المرور غير صحيحة.");
        }

        user.ResetLockout();
        await _userRepository.UpdateAsync(user, cancellationToken);

        var (token, expiresAtUtc) = _tokenService.GenerateToken(user);

        return new LoginResult(token, expiresAtUtc);
    }
}

