using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class EntityHistoryService : IEntityHistoryService
{
    private readonly IEventStore eventStore;
    private readonly IEventProcessor eventProcessor;

    public EntityHistoryService(IEventStore eventStore, IEventProcessor eventProcessor)
    {
        this.eventStore = eventStore;
        this.eventProcessor = eventProcessor;
    }

    public async Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid id)
        where T : Entity =>
        (await GetEntityHistoryWithEventsAsync<T>(id)).Select(e => e.entity).ToList();

    public async Task<IReadOnlyCollection<(T entity, Event e)>> GetEntityHistoryWithEventsAsync<T>(
        Guid id
    )
        where T : Entity
    {
        var events = await eventStore.GetEventsAsync(id);
        var entitiesWithEvents = new List<(T, Event)>();

        foreach (var e in events)
        {
            var entity = await eventProcessor.ProcessHistoryAsync(e);
            entitiesWithEvents.Add(((T)entity, e));
        }

        return entitiesWithEvents;
    }
}
