namespace EventSource.Core.Interfaces;

public interface IEventHandler<TEvent> where TEvent : Event
{
    Task HandleAsync(TEvent e);
}