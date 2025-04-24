namespace EventSource.Core.Events;

public interface IDeleteEvent<out T> : IEvent
    where T : IEntity
{
    T Entity { get; }
}
