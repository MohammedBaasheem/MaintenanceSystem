namespace MaintenanceSystem.Application.Features.Users.CreateUser;

public record CreateUserResponse(Guid Id, string UserName, string Email, string Role, string TemporaryPassword);

