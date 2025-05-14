using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Core.Interfaces;

public interface IEventStore
{
    Task InsertEventAsync(IEvent e);
    Task<IEvent?> GetEventByIdAsync(Guid id);
    Task<IReadOnlyCollection<IEvent>> GetEventsAsync();
    Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(DateTime until);
    Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(DateTime from);
    Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(DateTime from, DateTime until);

    Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdAsync(Guid entityId);
    Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdUntilAsync(Guid entityId, DateTime until);
    Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until
    );
    Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(long fromEventNumber);
    Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(long untilEventNumber);
    Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(
        long fromEventNumber,
        long untilEventNumber
    );
}
