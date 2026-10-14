using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Application.Common.Security;
using MediatR;

namespace MaintenanceSystem.Application.Features.Users.ResetUserPassword;

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, ResetUserPasswordResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ResetUserPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ResetUserPasswordResponse> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("المستخدم غير موجود.");
        }

        var temporaryPassword = TemporaryPasswordGenerator.Generate(12);
        var passwordHash = _passwordHasher.Hash(temporaryPassword);

        user.UpdatePassword(passwordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new ResetUserPasswordResponse(temporaryPassword);
    }
}

