namespace EventSource.Core;

public abstract record Event
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; init; }
    private Guid? entityId;

    public Guid? EntityId
    {
        get => entityId;
        set
        {
            if (entityId is not null && entityId != Guid.Empty)
                throw new InvalidOperationException(
                    "Entity id is already set and cannot be changed"
                );

            entityId = value;
        }
    }

    protected Event()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }

    protected Event(Guid entityId)
    {
        if (entityId == Guid.Empty)
            throw new ArgumentException($"Entity id cannot be empty", nameof(entityId));

        EntityId = entityId;
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }
}
