using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Core.Models.Events;

public interface IDeleteEvent<out T> : IEvent
    where T : IEntity
{
    T Entity { get; }
}