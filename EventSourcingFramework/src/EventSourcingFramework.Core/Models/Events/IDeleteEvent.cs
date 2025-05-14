using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Core.Models.Events;

public interface IMongoDeleteEvent<out T> : IEvent
    where T : IEntity
{
    T Entity { get; }
}
