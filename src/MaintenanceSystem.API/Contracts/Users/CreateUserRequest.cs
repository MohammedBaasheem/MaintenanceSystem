namespace MaintenanceSystem.API.Contracts.Users;

public record CreateUserRequest(string UserName, string Email, string Role);

