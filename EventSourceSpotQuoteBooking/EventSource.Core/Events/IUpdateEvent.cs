namespace EventSource.Core.Events;

public interface IUpdateEvent<out T> : IEvent
    where T : IEntity
{
    T Entity { get; }
}
