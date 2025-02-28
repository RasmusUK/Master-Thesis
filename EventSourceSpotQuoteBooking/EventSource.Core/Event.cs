namespace EventSource.Core;

public abstract class Event
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Guid AggregateId { get; init; }

    protected Event(Guid aggregateId)
    {
        if (aggregateId == Guid.Empty)
            throw new ArgumentException("AggregateId cannot be empty", nameof(aggregateId));

        AggregateId = aggregateId;
    }
}
