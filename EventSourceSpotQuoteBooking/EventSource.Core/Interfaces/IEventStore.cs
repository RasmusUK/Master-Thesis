namespace EventSource.Core.Interfaces;

public interface IEventStore
{
    Task InsertEventAsync(Event e);
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<IReadOnlyCollection<Event>> GetEventsAsync();
    Task<IReadOnlyCollection<Event>> GetEventsUntilAsync(DateTime until);
    Task<IReadOnlyCollection<Event>> GetEventsFromUntilAsync(DateTime from, DateTime until);

    Task<IReadOnlyCollection<Event>> GetEventsByEntityIdAsync(Guid entityId);
    Task<IReadOnlyCollection<Event>> GetEventsByEntityIdUntilAsync(Guid entityId, DateTime until);
    Task<IReadOnlyCollection<Event>> GetEventsByEntityIdFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until
    );
}
