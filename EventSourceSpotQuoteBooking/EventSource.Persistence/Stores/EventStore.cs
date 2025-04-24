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
    private readonly IPersonalDataInterceptor personalDataInterceptor;
    private readonly IEventSequenceGenerator sequenceGenerator;

    public EventStore(
        IMongoDbService mongoDbService,
        IPersonalDataInterceptor personalDataInterceptor,
        IEventSequenceGenerator sequenceGenerator
    )
    {
        this.personalDataInterceptor = personalDataInterceptor;
        this.sequenceGenerator = sequenceGenerator;
        collection = mongoDbService.EventCollection;
    }

    public async Task InsertEventAsync(IEvent e)
    {
        if (e is not EventBase eventBase)
            throw new ArgumentException("Event must be of type EventBase", nameof(e));

        if (await EventExistsAsync(eventBase))
            return;

        eventBase.EventNumber = await sequenceGenerator.GetNextSequenceNumberAsync();
        eventBase = (EventBase)await personalDataInterceptor.ProcessEventForStorage(eventBase);
        await collection.InsertOneAsync(eventBase);
    }

    public async Task<IEvent?> GetEventByIdAsync(Guid id)
    {
        var mongoEvent = await collection.Find(e => e.Id == id).FirstOrDefaultAsync();
        if (mongoEvent is null)
            return null;

        return await personalDataInterceptor.ProcessEventForRetrieval(mongoEvent);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsAsync()
    {
        var mongoEvents = await collection.Find(_ => true).ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(DateTime until)
    {
        var mongoEvents = await collection.Find(e => e.Timestamp <= until).ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(DateTime from)
    {
        var mongoEvents = await collection.Find(e => e.Timestamp >= from).ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(
        DateTime from,
        DateTime until
    )
    {
        var mongoEvents = await collection
            .Find(e => e.Timestamp >= from && e.Timestamp <= until)
            .ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdAsync(Guid entityId)
    {
        var mongoEvents = await collection.Find(e => e.EntityId == entityId).ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdUntilAsync(
        Guid entityId,
        DateTime until
    )
    {
        var mongoEvents = await collection
            .Find(e => e.EntityId == entityId && e.Timestamp <= until)
            .ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until
    )
    {
        var mongoEvents = await collection
            .Find(e => e.EntityId == entityId && e.Timestamp >= from && e.Timestamp <= until)
            .ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(long fromEventNumber)
    {
        var mongoEvents = await collection
            .Find(e => e.EventNumber >= fromEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(long untilEventNumber)
    {
        var mongoEvents = await collection
            .Find(e => e.EventNumber <= untilEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(
        long fromEventNumber,
        long untilEventNumber
    )
    {
        var mongoEvents = await collection
            .Find(e => e.EventNumber >= fromEventNumber && e.EventNumber <= untilEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
        return await ToDomain(mongoEvents);
    }

    private async Task<IReadOnlyCollection<IEvent>> ToDomain(ICollection<EventBase> mongoDbEvents)
    {
        var processedEvents = await Task.WhenAll(
            mongoDbEvents.Select(e => personalDataInterceptor.ProcessEventForRetrieval(e))
        );

        return processedEvents.ToList();
    }

    private Task<bool> EventExistsAsync(EventBase e) =>
        collection.Find(x => x.Id == e.Id).AnyAsync();
}
