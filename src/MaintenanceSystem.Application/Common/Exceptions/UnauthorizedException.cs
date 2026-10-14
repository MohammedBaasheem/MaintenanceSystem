namespace MaintenanceSystem.Application.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Invalid username or password.") : base(message)
    {
    }
}

