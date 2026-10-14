namespace MaintenanceSystem.Domain.Exceptions;

public class InvalidStateTransitionException : DomainException
{
    public InvalidStateTransitionException(string message) : base(message)
    {
    }

    public InvalidStateTransitionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

