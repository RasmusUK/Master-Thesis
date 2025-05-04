using EventSource.Core;

namespace EventSource.Persistence;

public abstract class Entity : IEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int ConcurrencyVersion { get; set; } = 1;
    public int SchemaVersion { get; set; } = 1;
}
