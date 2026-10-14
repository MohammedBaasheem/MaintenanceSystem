namespace MaintenanceSystem.Application.Common.Exceptions;

public class LockedException : Exception
{
    public LockedException(string message = "Account is locked due to multiple failed login attempts. Please try again later.") : base(message)
    {
    }
}

