namespace MaintenanceSystem.API.Contracts.Users;

public record CreateUserResponse(Guid Id, string UserName, string Email, string Role, string TemporaryPassword);

