namespace EventSource.Core.Interfaces;

public interface IEventStore
{
    Task SaveEventAsync(Event e);
    Task<IReadOnlyCollection<Event>> GetEventsAsync();
    Task<IReadOnlyCollection<Event>> GetEventsAsync(Guid aggregateId);
}
