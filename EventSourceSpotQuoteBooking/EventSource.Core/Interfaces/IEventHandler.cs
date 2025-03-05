namespace EventSource.Core.Interfaces;

public interface IEventHandler<in TEvent>
    where TEvent : Event
{
    Task HandleAsync(TEvent e);
}
