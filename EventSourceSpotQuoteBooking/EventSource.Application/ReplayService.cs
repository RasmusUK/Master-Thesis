using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Events;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class ReplayService : IReplayService
{
    private readonly IEventStore eventStore;
    private readonly IEventProcessor eventProcessor;
    private readonly IEntityStore entityStore;

    public ReplayService(
        IEventStore eventStore,
        IEventProcessor eventProcessor,
        IEntityStore entityStore
    )
    {
        this.eventStore = eventStore;
        this.eventProcessor = eventProcessor;
        this.entityStore = entityStore;
    }

    public async Task ReplayAllEventsAsync()
    {
        var events = await eventStore.GetEventsAsync();
        await ProcessReplayEventsAsync(events);
    }

    public async Task ReplayAllEventsUntilAsync(DateTime until)
    {
        var events = await eventStore.GetEventsUntilAsync(until);
        await ProcessReplayEventsAsync(events);
    }

    public async Task ReplayAllEventsFromUntilAsync(DateTime from, DateTime until)
    {
        var events = await eventStore.GetEventsFromUntilAsync(from, until);
        await ProcessReplayEventsAsync(events);
    }

    public async Task ReplayAllEntityEventsAsync(Guid entityId)
    {
        var events = await eventStore.GetEventsByEntityIdAsync(entityId);
        await ProcessReplayEventsAsync(events);
    }

    public async Task ReplayAllEntityEventsUntilAsync(Guid entityId, DateTime until)
    {
        var events = await eventStore.GetEventsByEntityIdUntilAsync(entityId, until);
        await ProcessReplayEventsAsync(events);
    }

    public async Task ReplayAllEntityEventsFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until
    )
    {
        var events = await eventStore.GetEventsByEntityIdFromUntilAsync(entityId, from, until);
        await ProcessReplayEventsAsync(events);
    }

    public Task ReplayEventsAsync(IReadOnlyCollection<Event> events) =>
        ProcessReplayEventsAsync(events);

    public Task ReplayEventAsync(Event e) => ProcessReplayEventsAsync(new[] { e });

    private async Task ProcessReplayEventsAsync(IReadOnlyCollection<Event> events)
    {
        foreach (var e in events)
        {
            await ProcessReplayEventAsync(e);
        }
    }

    private Task ProcessReplayEventAsync(Event e)
    {
        dynamic dynEvent = e;
        if (
            e.GetType().IsGenericType
            && e.GetType().GetGenericTypeDefinition() == typeof(CreateEvent<>)
        )
            return ProcessReplayCreateEvent(dynEvent);

        if (
            e.GetType().IsGenericType
            && e.GetType().GetGenericTypeDefinition() == typeof(UpdateEvent<>)
        )
            return ProcessReplayUpdateEvent(dynEvent);

        if (
            e.GetType().IsGenericType
            && e.GetType().GetGenericTypeDefinition() == typeof(DeleteEvent<>)
        )
            return ProcessReplayDeleteEvent(dynEvent);

        return eventProcessor.ProcessAsync(e);
    }

    private Task ProcessReplayCreateEvent<T>(CreateEvent<T> e)
        where T : Entity => entityStore.UpsertEntityAsync(e.Entity);

    private Task ProcessReplayUpdateEvent<T>(UpdateEvent<T> e)
        where T : Entity => entityStore.UpsertEntityAsync(e.Entity);

    private Task ProcessReplayDeleteEvent<T>(DeleteEvent<T> e)
        where T : Entity => entityStore.DeleteEntityAsync(e.Entity);
}
