namespace EventSourcingFramework.Core;

public interface IEntity
{
    Guid Id { get; }
    int ConcurrencyVersion { get; set; }
}
