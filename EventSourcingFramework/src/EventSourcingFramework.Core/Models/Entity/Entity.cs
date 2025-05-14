namespace EventSourcingFramework.Core.Models.Entity;

public abstract class Entity : IEntity
{
    public int SchemaVersion { get; set; } = 1;
    public Guid Id { get; set; } = Guid.NewGuid();
    public int ConcurrencyVersion { get; set; } = 1;
}