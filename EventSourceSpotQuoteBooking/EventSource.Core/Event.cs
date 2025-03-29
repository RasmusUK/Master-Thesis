namespace EventSource.Core;

public abstract record Event
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid? EntityId { get; set; }

    protected Event() { }

    protected Event(Guid entityId)
    {
        EntityId = entityId;
    }
}
