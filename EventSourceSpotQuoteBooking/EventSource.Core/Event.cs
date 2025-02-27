namespace EventSource.Core;

public abstract class Event
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
    public Guid AggregateId { get; private set; }

    protected Event(Guid aggregateId)
    {
        if (aggregateId == Guid.Empty)
            throw new ArgumentException("AggregateId cannot be empty", nameof(aggregateId));

        AggregateId = aggregateId;
    }
}
