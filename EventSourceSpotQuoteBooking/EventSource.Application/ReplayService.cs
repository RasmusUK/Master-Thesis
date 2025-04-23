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
    private readonly IGlobalReplayContext replayContext;

    public ReplayService(
        IEventStore eventStore,
        IEventProcessor eventProcessor,
        IEntityStore entityStore,
        IGlobalReplayContext replayContext
    )
    {
        this.eventStore = eventStore;
        this.eventProcessor = eventProcessor;
        this.entityStore = entityStore;
        this.replayContext = replayContext;
    }

    public void StartReplay(ReplayMode mode = ReplayMode.Strict) => replayContext.StartReplay(mode);

    public async Task StopReplay()
    {
        replayContext.StopReplay();
        await ReplayAllAsync();
    }

    public async Task ReplayAllAsync(bool autoStop = true)
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsAsync();
        await ProcessReplayEventsAsync(events);
        if (autoStop)
            replayContext.StopReplay();
    }

    public async Task ReplayUntilAsync(DateTime until, bool autoStop = true)
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsUntilAsync(until);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayFromAsync(DateTime from, bool autoStop = true)
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsUntilAsync(from);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayFromUntilAsync(DateTime from, DateTime until, bool autoStop = true)
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsFromUntilAsync(from, until);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayEntityAsync(Guid entityId, bool autoStop = true)
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsByEntityIdAsync(entityId);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayEntityUntilAsync(Guid entityId, DateTime until, bool autoStop = true)
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsByEntityIdUntilAsync(entityId, until);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayEntityFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until,
        bool autoStop = true
    )
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsByEntityIdFromUntilAsync(entityId, from, until);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayEventsAsync(IReadOnlyCollection<Event> events, bool autoStop = true)
    {
        StartReplayIfNeeded();
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public Task ReplayEventAsync(Event e, bool autoStop = true) =>
        ReplayEventsAsync(new[] { e }, autoStop);

    public IReadOnlyCollection<Event> GetSimulatedEvents() => replayContext.GetEvents();

    public bool IsRunning() => replayContext.IsReplaying;

    private void StartReplayIfNeeded()
    {
        if (!replayContext.IsReplaying)
            replayContext.StartReplay();
    }

    private async Task StopReplayIfNeeded(bool autoStop)
    {
        if (autoStop)
            await StopReplay();
    }

    private async Task ProcessReplayEventsAsync(IEnumerable<Event> events)
    {
        foreach (var e in events)
        {
            dynamic dynEvent = e;
            if (
                e.GetType().IsGenericType
                && e.GetType().GetGenericTypeDefinition() == typeof(CreateEvent<>)
            )
                await ProcessReplayCreateEvent(dynEvent);
            else if (
                e.GetType().IsGenericType
                && e.GetType().GetGenericTypeDefinition() == typeof(UpdateEvent<>)
            )
                await ProcessReplayUpdateEvent(dynEvent);
            else if (
                e.GetType().IsGenericType
                && e.GetType().GetGenericTypeDefinition() == typeof(DeleteEvent<>)
            )
                await ProcessReplayDeleteEvent(dynEvent);
            else
                await eventProcessor.ProcessAsync(e);
        }
    }

    private Task ProcessReplayCreateEvent<T>(CreateEvent<T> e)
        where T : Entity => entityStore.UpsertEntityAsync(e.Entity);

    private Task ProcessReplayUpdateEvent<T>(UpdateEvent<T> e)
        where T : Entity => entityStore.UpsertEntityAsync(e.Entity);

    private Task ProcessReplayDeleteEvent<T>(DeleteEvent<T> e)
        where T : Entity => entityStore.DeleteEntityAsync(e.Entity);
}
