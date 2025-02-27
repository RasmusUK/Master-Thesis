namespace EventSource.Core.Interfaces;

public interface IEventProcessor
{
    Task ProcessAsync(Event e);
}