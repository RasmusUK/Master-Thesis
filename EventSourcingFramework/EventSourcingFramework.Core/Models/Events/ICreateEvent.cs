namespace EventSourcingFramework.Core.Models.Events;

public interface IMongoCreateEvent<out T> : IEvent
    where T : IEntity
{
    T Entity { get; }
}
