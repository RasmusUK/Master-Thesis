using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Events;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class ReplayService : IReplayService
{
    private readonly IEventStore eventStore;
    private readonly IEntityStore entityStore;
    private readonly IGlobalReplayContext replayContext;

    public ReplayService(
        IEventStore eventStore,
        IEntityStore entityStore,
        IGlobalReplayContext replayContext
    )
    {
        this.eventStore = eventStore;
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

    public async Task ReplayEventsAsync(IReadOnlyCollection<IEvent> events, bool autoStop = true)
    {
        StartReplayIfNeeded();
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public Task ReplayEventAsync(IEvent e, bool autoStop = true) =>
        ReplayEventsAsync(new[] { e }, autoStop);

    public IReadOnlyCollection<IEvent> GetSimulatedEvents() => replayContext.GetEvents();

    public async Task ReplayFromEventNumberAsync(long fromEventNumber, bool autoStop = true)
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsFromAsync(fromEventNumber);
        await ProcessReplayEventsAsync(events);
        if (autoStop)
            await StopReplay();
    }

    public async Task ReplayUntilEventNumberAsync(long untilEventNumber, bool autoStop = true)
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsUntilAsync(untilEventNumber);
        await ProcessReplayEventsAsync(events);
        if (autoStop)
            await StopReplay();
    }

    public async Task ReplayFromUntilEventNumberAsync(
        long fromEventNumber,
        long untilEventNumber,
        bool autoStop = true
    )
    {
        StartReplayIfNeeded();
        var events = await eventStore.GetEventsFromUntilAsync(fromEventNumber, untilEventNumber);
        await ProcessReplayEventsAsync(events);
        if (autoStop)
            await StopReplay();
    }

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

    private async Task ProcessReplayEventsAsync(IEnumerable<IEvent> events)
    {
        foreach (var e in events)
        {
            switch (e)
            {
                case ICreateEvent<IEntity> create:
                    await ProcessReplayCreateEventDynamic(create);
                    break;
                case IUpdateEvent<IEntity> update:
                    await ProcessReplayUpdateEventDynamic(update);
                    break;
                case IDeleteEvent<IEntity> delete:
                    await ProcessReplayDeleteEventDynamic(delete);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unknown event type: {e.GetType().Name}. Cannot process replay."
                    );
            }
        }
    }

    private Task ProcessReplayCreateEventDynamic(ICreateEvent<IEntity> e) =>
        ProcessReplayCreateEvent((dynamic)e);

    private Task ProcessReplayUpdateEventDynamic(IUpdateEvent<IEntity> e) =>
        ProcessReplayUpdateEvent((dynamic)e);

    private Task ProcessReplayDeleteEventDynamic(IDeleteEvent<IEntity> e) =>
        ProcessReplayDeleteEvent((dynamic)e);

    private Task ProcessReplayCreateEvent<T>(ICreateEvent<T> e)
        where T : IEntity => entityStore.UpsertEntityAsync(e.Entity);

    private Task ProcessReplayUpdateEvent<T>(IUpdateEvent<T> e)
        where T : IEntity => entityStore.UpsertEntityAsync(e.Entity);

    private Task ProcessReplayDeleteEvent<T>(IDeleteEvent<T> e)
        where T : IEntity => entityStore.DeleteEntityAsync(e.Entity);
}
