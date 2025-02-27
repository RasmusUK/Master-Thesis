namespace EventSource.Core;

public abstract class AggregateRoot
{
    public Guid Id { get; init; } = Guid.NewGuid();

    protected AggregateRoot() { }

    protected AggregateRoot(Guid id)
    {
        Id = id;
    }

    public abstract void Apply(Event e);
}
