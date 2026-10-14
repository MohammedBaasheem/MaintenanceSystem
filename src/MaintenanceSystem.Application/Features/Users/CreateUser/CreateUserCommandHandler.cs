using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Application.Common.Security;
using MaintenanceSystem.Domain.Entities;
using MediatR;

namespace MaintenanceSystem.Application.Features.Users.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByUserNameAsync(request.UserName.Trim(), cancellationToken))
        {
            throw new ConflictException($"اسم المستخدم '{request.UserName.Trim()}' مسجل مسبقاً.");
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email.Trim(), cancellationToken))
        {
            throw new ConflictException($"البريد الإلكتروني '{request.Email.Trim()}' مسجل مسبقاً.");
        }

        var role = await _roleRepository.GetByNameAsync(request.Role.Trim(), cancellationToken);
        if (role == null)
        {
            throw new NotFoundException($"الدور '{request.Role.Trim()}' غير موجود.");
        }

        var temporaryPassword = TemporaryPasswordGenerator.Generate(12);
        var passwordHash = _passwordHasher.Hash(temporaryPassword);

        var user = new User(request.UserName, request.Email, passwordHash);
        user.AssignRole(role.Id);

        await _userRepository.AddAsync(user, cancellationToken);

        return new CreateUserResponse(user.Id, user.UserName, user.Email, role.Name, temporaryPassword);
    }
}

