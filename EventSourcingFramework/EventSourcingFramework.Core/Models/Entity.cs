namespace EventSourcingFramework.Core.Models;

public abstract class Entity : IEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int ConcurrencyVersion { get; set; } = 1;
    public int SchemaVersion { get; set; } = 1;
}
