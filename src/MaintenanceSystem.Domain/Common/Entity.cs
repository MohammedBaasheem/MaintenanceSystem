namespace MaintenanceSystem.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }

    protected Entity()
    {
        Id = Guid.CreateVersion7();
        CreatedAtUtc = DateTime.UtcNow;
    }

    protected Entity(Guid id, DateTime createdAtUtc)
    {
        Id = id;
        CreatedAtUtc = createdAtUtc;
    }
}

