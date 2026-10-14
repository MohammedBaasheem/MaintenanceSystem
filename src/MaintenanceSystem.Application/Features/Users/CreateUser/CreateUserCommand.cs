using MediatR;

namespace MaintenanceSystem.Application.Features.Users.CreateUser;

public record CreateUserCommand(string UserName, string Email, string Role) : IRequest<CreateUserResponse>;

