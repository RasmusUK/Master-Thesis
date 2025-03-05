namespace EventSource.Core;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    protected Entity() { }

    public abstract void Apply(Event e);
}
