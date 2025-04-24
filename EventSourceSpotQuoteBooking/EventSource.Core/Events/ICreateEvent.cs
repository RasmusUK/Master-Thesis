namespace EventSource.Core.Events;

public interface ICreateEvent<out T> : IEvent
    where T : IEntity
{
    T Entity { get; }
}
