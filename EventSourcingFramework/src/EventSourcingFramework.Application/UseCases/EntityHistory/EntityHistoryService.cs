using EventSourcingFramework.Application.Abstractions.EntityHistory;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Core.Models.Entity;
using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Application.UseCases.EntityHistory;

public class EntityHistoryService : IEntityHistoryService
{
    private readonly IEventStore eventStore;

    public EntityHistoryService(IEventStore eventStore)
    {
        this.eventStore = eventStore;
    }

    public async Task<IReadOnlyCollection<T>> GetEntityHistoryAsync<T>(Guid id)
        where T : IEntity
    {
        return (await GetEntityHistoryWithEventsAsync<T>(id)).Select(e => e.entity).ToList();
    }

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