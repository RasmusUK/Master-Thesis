namespace EventSourcingFramework.Core.Models.Entity;

public interface IEntity
{
    Guid Id { get; }
    int ConcurrencyVersion { get; set; }
    public int SchemaVersion { get; set; }
}