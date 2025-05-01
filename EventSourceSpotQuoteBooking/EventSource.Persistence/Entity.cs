using EventSource.Core;

namespace EventSource.Persistence;

public class Entity : IEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int ConcurrencyVersion { get; set; } = 1;
}
