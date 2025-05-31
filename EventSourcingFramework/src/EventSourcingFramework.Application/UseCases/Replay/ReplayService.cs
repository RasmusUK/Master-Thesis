using EventSourcingFramework.Application.Abstractions.Replay;
using EventSourcingFramework.Application.Abstractions.ReplayContext;
using EventSourcingFramework.Application.Abstractions.Snapshots;
using EventSourcingFramework.Core.Enums;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Core.Models.Entity;
using EventSourcingFramework.Core.Models.Events;

namespace EventSourcingFramework.Application.UseCases.Replay;

public class ReplayService : IReplayService
{
    private readonly IEntityStore entityStore;
    private readonly IEventStore eventStore;
    private readonly IReplayContext replayContext;
    private readonly IReplayEnvironmentSwitcher replayEnvironmentSwitcher;
    private readonly ISnapshotService snapshotService;
    private readonly ISnapshotSettings snapshotSettings;

    public ReplayService(
        IEventStore eventStore,
        IEntityStore entityStore,
        IReplayContext replayContext,
        ISnapshotService snapshotService,
        IReplayEnvironmentSwitcher replayEnvironmentSwitcher, ISnapshotSettings snapshotSettings)
    {
        this.eventStore = eventStore;
        this.entityStore = entityStore;
        this.replayContext = replayContext;
        this.snapshotService = snapshotService;
        this.replayEnvironmentSwitcher = replayEnvironmentSwitcher;
        this.snapshotSettings = snapshotSettings;
    }

    public async Task StartReplayAsync(ReplayMode mode = ReplayMode.Replay)
    {
        replayContext.StartReplay(mode);
        if (mode == ReplayMode.Debug)
            await replayEnvironmentSwitcher.UseDebugDatabaseAsync();
        replayContext.IsLoading = true;
        await TryTakeSnapshotAsync();
        replayContext.IsLoading = false;
    }

    public async Task StopReplayAsync()
    {
        IReadOnlyCollection<IEvent> events;
        var latestSnapshot = await TryGetLastSnapshotAsync();
        if (latestSnapshot is not null)
        {
            await TryRestoreSnapshotAsync(latestSnapshot.SnapshotId);
            events = await eventStore.GetEventsFromAsync(latestSnapshot.EventNumber);
        }
        else
        {
            events = await eventStore.GetEventsAsync();
        }

        await ProcessReplayEventsAsync(events);
        if (replayContext.ReplayMode == ReplayMode.Debug)
            await replayEnvironmentSwitcher.UseProductionDatabaseAsync();
        replayContext.StopReplay();
    }

    public async Task ReplayAllAsync(bool autoStop = true, bool useSnapshot = true)
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        await TryRestoreSnapshotAsync(useSnapshot);
        IReadOnlyCollection<IEvent> events;
        if (useSnapshot)
        {
            var latestSnapshot = await TryGetLastSnapshotAsync();
            if (latestSnapshot is null)
                events = await eventStore.GetEventsAsync();
            else
                events = await eventStore.GetEventsFromAsync(latestSnapshot.EventNumber);
        }
        else
        {
            events = await eventStore.GetEventsAsync();
        }

        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayUntilAsync(
        DateTime until,
        bool autoStop = true,
        bool useSnapshot = true
    )
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        await TryRestoreSnapshotAsync(useSnapshot, until: until);
        IReadOnlyCollection<IEvent> events = new List<IEvent>();
        if (useSnapshot)
        {
            var latestSnapshot = await TryGetLastSnapshotAsync();
            if (latestSnapshot is not null && latestSnapshot.Timestamp <= until)
            {
                events = await eventStore.GetEventsFromUntilAsync(latestSnapshot.Timestamp, until);
            }
        }
        else
        {
            events = await eventStore.GetEventsUntilAsync(until);
        }
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayFromAsync(DateTime from, bool autoStop = true, bool useSnapshot = true)
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        await TryRestoreSnapshotAsync(useSnapshot, from);
        var events = await eventStore.GetEventsFromAsync(from);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayFromUntilAsync(
        DateTime from,
        DateTime until,
        bool autoStop = true,
        bool useSnapshot = true
    )
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        await TryRestoreSnapshotAsync(useSnapshot, from);
        var events = await eventStore.GetEventsFromUntilAsync(from, until);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayEntityAsync(
        Guid entityId,
        bool autoStop = true,
        bool useSnapshot = true
    )
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        var events = await eventStore.GetEventsByEntityIdAsync(entityId);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayEntityUntilAsync(
        Guid entityId,
        DateTime until,
        bool autoStop = true,
        bool useSnapshot = true
    )
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        var events = await eventStore.GetEventsByEntityIdUntilAsync(entityId, until);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayEntityFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until,
        bool autoStop = true,
        bool useSnapshot = true
    )
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        var events = await eventStore.GetEventsByEntityIdFromUntilAsync(entityId, from, until);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayEventsAsync(IReadOnlyCollection<IEvent> events, bool autoStop = true)
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        await ProcessReplayEventsAsync(events);
    }

    public Task ReplayEventAsync(IEvent e, bool autoStop = true)
    {
        return ReplayEventsAsync(new[] { e }, autoStop);
    }

    public IReadOnlyCollection<IEvent> GetSimulatedEvents()
    {
        return replayContext.GetEvents();
    }

    public async Task ReplayFromEventNumberAsync(
        long fromEventNumber,
        bool autoStop = true,
        bool useSnapshot = true
    )
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        await TryRestoreSnapshotAsync(useSnapshot, fromNumber: fromEventNumber);
        var events = await eventStore.GetEventsFromAsync(fromEventNumber);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayUntilEventNumberAsync(
        long untilEventNumber,
        bool autoStop = true,
        bool useSnapshot = true
    )
    {
        await StartReplayIfNeeded(ReplayMode.Replay);
        await TryRestoreSnapshotAsync(useSnapshot, untilNumber: untilEventNumber);
        var events = await eventStore.GetEventsUntilAsync(untilEventNumber);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public async Task ReplayFromUntilEventNumberAsync(
        long fromEventNumber,
        long untilEventNumber,
        bool autoStop = true,
        bool useSnapshot = true
    )
    {
        await StartReplayIfNeeded(ReplayMode.Sandbox);
        await TryRestoreSnapshotAsync(useSnapshot, fromNumber: fromEventNumber);
        var events = await eventStore.GetEventsFromUntilAsync(fromEventNumber, untilEventNumber);
        await ProcessReplayEventsAsync(events);
        await StopReplayIfNeeded(autoStop);
    }

    public bool IsRunning()
    {
        return replayContext.IsReplaying;
    }

    private async Task StartReplayIfNeeded(ReplayMode replayMode = ReplayMode.Strict)
    {
        if (!replayContext.IsReplaying)
            await StartReplayAsync(replayMode);
    }

    private async Task StopReplayIfNeeded(bool autoStop)
    {
        if (autoStop)
            await StopReplayAsync();
    }

    private async Task TryRestoreSnapshotAsync(
        bool useSnapshot,
        DateTime? from = null,
        DateTime? until = null,
        long? fromNumber = null,
        long? untilNumber = null
    )
    {
        if (!useSnapshot || !snapshotSettings.Enabled)
            return;

        string? snapshotId;

        if (fromNumber.HasValue)
            snapshotId = await snapshotService.GetLatestSnapshotBeforeAsync(fromNumber.Value);
        else if (untilNumber.HasValue)
            snapshotId = await snapshotService.GetLatestSnapshotBeforeAsync(untilNumber.Value);
        else if (from.HasValue)
            snapshotId = await snapshotService.GetLatestSnapshotBeforeAsync(from.Value);
        else if (until.HasValue)
            snapshotId = await snapshotService.GetLatestSnapshotBeforeAsync(until.Value);
        else
            snapshotId = await snapshotService.GetLastSnapshotIdAsync();

        if (snapshotId is not null)
            await snapshotService.RestoreSnapshotAsync(snapshotId);
    }

    private async Task<SnapshotMetadata?> TryGetLastSnapshotAsync()
    {
        if (!snapshotSettings.Enabled)
            return null;
        return await snapshotService.GetLastSnapshotAsync();
    }

    private async Task TryRestoreSnapshotAsync(string snapshotId)
    {
        if (!snapshotSettings.Enabled)
            return;
        await snapshotService.RestoreSnapshotAsync(snapshotId);
    }

    private async Task TryTakeSnapshotAsync()
    {
        if (!snapshotSettings.Enabled)
            return;
        await snapshotService.TakeSnapshotAsync(await eventStore.GetCurrentSequenceNumberAsync());
    }

    private async Task ProcessReplayEventsAsync(IEnumerable<IEvent> events)
    {
        foreach (var e in events)
            switch (e)
            {
                case ICreateEvent<IEntity> create:
                    await ProcessReplayCreateEvent((dynamic)create);
                    break;
                case IUpdateEvent<IEntity> update:
                    await ProcessReplayUpdateEvent((dynamic)update);
                    break;
                case IDeleteEvent<IEntity> delete:
                    await ProcessReplayDeleteEvent((dynamic)delete);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown event type: {e.GetType().Name}");
            }
    }
    
    private Task ProcessReplayCreateEvent<T>(ICreateEvent<T> e)
        where T : IEntity
    {
        return entityStore.UpsertEntityAsync(e.Entity);
    }

    private Task ProcessReplayUpdateEvent<T>(IUpdateEvent<T> e)
        where T : IEntity
    {
        return entityStore.UpsertEntityAsync(e.Entity);
    }

    private Task ProcessReplayDeleteEvent<T>(IDeleteEvent<T> e)
        where T : IEntity
    {
        return entityStore.DeleteEntityAsync(e.Entity);
    }
}