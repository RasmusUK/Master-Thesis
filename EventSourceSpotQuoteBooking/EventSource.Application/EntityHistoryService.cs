using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Events;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EntityHistoryService : IEntityHistoryService
{
    private readonly IEventStore eventStore;

    public EntityHistoryService(IEventStore eventStore)
    {
        this.eventStore = eventStore;
    }

    public async Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid id)
        where T : IEntity =>
        (await GetEntityHistoryWithEventsAsync<T>(id)).Select(e => e.entity).ToList();

    public async Task<IReadOnlyCollection<(T entity, IEvent e)>> GetEntityHistoryWithEventsAsync<T>(
        Guid id
    )
        where T : IEntity
    {
        var events = await eventStore.GetEventsByEntityIdAsync(id);
        var entitiesWithEvents = new List<(T, IEvent)>();

        foreach (var e in events)
        {
            var entity = GetEntityFromRepoEvent<T>(e);
            entitiesWithEvents.Add((entity, e));
        }

        return entitiesWithEvents;
    }

    private static T GetEntityFromRepoEvent<T>(IEvent e)
        where T : IEntity
    {
        dynamic dynEvent = e;
        return (T)dynEvent.Entity;
    }
}
