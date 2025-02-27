namespace EventSource.Core;

public interface IEventProcessor
{
    Task ProcessAsync(Event e);
}