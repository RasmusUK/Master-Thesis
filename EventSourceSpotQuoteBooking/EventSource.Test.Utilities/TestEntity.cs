using EventSource.Core;

namespace EventSource.Test.Utilities;

public record TestEntity : IEntity
{
    public Guid Id { get; set; }
    public int ConcurrencyVersion { get; set; }
    public string Name { get; set; }
}
