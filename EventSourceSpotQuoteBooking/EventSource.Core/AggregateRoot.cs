namespace EventSource.Core;

public abstract class AggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
}