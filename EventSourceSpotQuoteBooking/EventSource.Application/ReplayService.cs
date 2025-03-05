using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Interfaces;

namespace EventSource.Application;

public class ReplayService : IReplayService
{
    private readonly IEventStore eventStore;
    private readonly IEventProcessor eventProcessor;

    public ReplayService(IEventStore eventStore, IEventProcessor eventProcessor)
    {
        this.eventStore = eventStore;
        this.eventProcessor = eventProcessor;
    }

    public async Task ReplayAllEventsAsync()
    {
        var events = await eventStore.GetEventsAsync();
        await ProcessEvents(events);
    }

    public async Task ReplayAllEventsUntilAsync(DateTime until)
    {
        var events = await eventStore.GetEventsUntilAsync(until);
        await ProcessEvents(events);
    }

    public async Task ReplayAllEventsFromUntilAsync(DateTime from, DateTime until)
    {
        var events = await eventStore.GetEventsFromUntilAsync(from, until);
        await ProcessEvents(events);
    }

    public async Task ReplayAllEntityEventsAsync(Guid entityId)
    {
        var events = await eventStore.GetEventsByEntityIdAsync(entityId);
        await ProcessEvents(events);
    }

    public async Task ReplayAllEntityEventsUntilAsync(Guid entityId, DateTime until)
    {
        var events = await eventStore.GetEventsByEntityIdUntilAsync(entityId, until);
        await ProcessEvents(events);
    }

    public async Task ReplayAllEntityEventsFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until
    )
    {
        var events = await eventStore.GetEventsByEntityIdFromUntilAsync(entityId, from, until);
        await ProcessEvents(events);
    }

    public Task ReplayEventsAsync(IReadOnlyCollection<Event> events) => ProcessEvents(events);

    public Task ReplayEventAsync(Event e) => eventProcessor.ProcessReplayAsync(e);

    private async Task ProcessEvents(IReadOnlyCollection<Event> events)
    {
        foreach (var e in events)
        {
            await eventProcessor.ProcessReplayAsync(e);
        }
    }
}
