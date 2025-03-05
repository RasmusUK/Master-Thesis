namespace EventSource.Core;

public abstract class Event
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid EntityId { get; init; }

    protected Event(Guid entityId)
    {
        if (entityId == Guid.Empty)
            throw new ArgumentException($"Entity id cannot be empty", nameof(entityId));

        EntityId = entityId;
    }
}
