namespace EventSource.Core;

public interface IEventStore
{
    Task SaveEventAsync(Event e);
    Task<List<Event>> GetEventsAsync();
}