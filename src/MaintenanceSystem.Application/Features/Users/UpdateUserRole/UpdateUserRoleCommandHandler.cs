using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MediatR;

namespace MaintenanceSystem.Application.Features.Users.UpdateUserRole;

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == request.UserId)
        {
            throw new ConflictException("لا يمكن للمسؤول تغيير دوره الشخصي.");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("المستخدم غير موجود.");
        }

        var newRole = await _roleRepository.GetByNameAsync(request.Role.Trim(), cancellationToken);
        if (newRole == null)
        {
            throw new NotFoundException($"الدور '{request.Role.Trim()}' غير موجود.");
        }

        foreach (var existingUserRole in user.UserRoles.ToList())
        {
            user.RemoveRole(existingUserRole.RoleId);
        }

        user.AssignRole(newRole.Id);

        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}

