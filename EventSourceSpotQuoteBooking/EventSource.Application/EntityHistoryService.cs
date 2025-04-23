using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Events;
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
        var events = await eventStore.GetEventsByEntityIdAsync(id);
        var entitiesWithEvents = new List<(T, Event)>();

        foreach (var e in events)
        {
            if (IsRepoEvent(e))
            {
                var entity = GetEntityFromRepoEvent<T>(e);
                entitiesWithEvents.Add((entity, e));
            }
            else
            {
                var entity = await eventProcessor.ProcessHistoryAsync(e);
                entitiesWithEvents.Add(((T)entity, e));
            }
        }

        return entitiesWithEvents;
    }

    private static bool IsRepoEvent(Event e)
    {
        var eventType = e.GetType();

        return eventType.IsGenericType && IsSubclassOfRepoEvent(eventType);
    }

    private static bool IsSubclassOfRepoEvent(Type? type)
    {
        while (type is not null && type != typeof(object))
        {
            var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (cur == typeof(RepoEvent<>))
                return true;

            type = type.BaseType;
        }

        return false;
    }

    private static T GetEntityFromRepoEvent<T>(Event e)
        where T : Entity
    {
        dynamic dynEvent = e;
        return (T)dynEvent.Entity;
    }
}
