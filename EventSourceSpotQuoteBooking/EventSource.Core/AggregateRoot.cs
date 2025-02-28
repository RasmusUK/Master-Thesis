namespace EventSource.Core;

public abstract class AggregateRoot
{
    public Guid Id { get; init; } = Guid.NewGuid();

    protected AggregateRoot() { }

    public abstract void Apply(Event e);
}
