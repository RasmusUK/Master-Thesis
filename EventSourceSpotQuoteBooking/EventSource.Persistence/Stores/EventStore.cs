using EventSource.Application.Interfaces;
using EventSource.Core.Events;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Events;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class EventStore : IEventStore
{
    private readonly IMongoCollection<EventBase> collection;
    private readonly IEventSequenceGenerator sequenceGenerator;
    private readonly IPersonalDataService personalDataService;

    public EventStore(
        IMongoDbService mongoDbService,
        IEventSequenceGenerator sequenceGenerator,
        IPersonalDataService personalDataService
    )
    {
        this.sequenceGenerator = sequenceGenerator;
        this.personalDataService = personalDataService;
        collection = mongoDbService.EventCollection;
    }

    public async Task InsertEventAsync(IEvent e)
    {
        if (e is not EventBase eventBase)
            throw new ArgumentException("Event must be of type EventBase", nameof(e));

        if (await EventExistsAsync(eventBase))
            return;

        eventBase.EventNumber = await sequenceGenerator.GetNextSequenceNumberAsync();

        await personalDataService.StripAndStoreAsync(eventBase);

        await collection.InsertOneAsync(eventBase);
    }

    public async Task<IEvent?> GetEventByIdAsync(Guid id)
    {
        var evt = await collection.Find(e => e.Id == id).FirstOrDefaultAsync();
        if (evt != null)
            await personalDataService.RestoreAsync(evt);
        return evt;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsAsync()
    {
        var events = await collection.Find(_ => true).ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(DateTime until)
    {
        var events = await collection.Find(e => e.Timestamp <= until).ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(DateTime from)
    {
        var events = await collection.Find(e => e.Timestamp >= from).ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(
        DateTime from,
        DateTime until
    )
    {
        var events = await collection
            .Find(e => e.Timestamp >= from && e.Timestamp <= until)
            .ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdAsync(Guid entityId)
    {
        var events = await collection.Find(e => e.EntityId == entityId).ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdUntilAsync(
        Guid entityId,
        DateTime until
    )
    {
        var events = await collection
            .Find(e => e.EntityId == entityId && e.Timestamp <= until)
            .ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until
    )
    {
        var events = await collection
            .Find(e => e.EntityId == entityId && e.Timestamp >= from && e.Timestamp <= until)
            .ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(long fromEventNumber)
    {
        var events = await collection
            .Find(e => e.EventNumber >= fromEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(long untilEventNumber)
    {
        var events = await collection
            .Find(e => e.EventNumber <= untilEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(
        long fromEventNumber,
        long untilEventNumber
    )
    {
        var events = await collection
            .Find(e => e.EventNumber >= fromEventNumber && e.EventNumber <= untilEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
        await Task.WhenAll(events.Select(e => personalDataService.RestoreAsync(e)));
        return events;
    }

    private Task<bool> EventExistsAsync(EventBase e) =>
        collection.Find(x => x.Id == e.Id).AnyAsync();
}
