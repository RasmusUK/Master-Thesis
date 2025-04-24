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

    public EventStore(IMongoDbService mongoDbService, IEventSequenceGenerator sequenceGenerator)
    {
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
        await collection.InsertOneAsync(eventBase);
    }

    public async Task<IEvent?> GetEventByIdAsync(Guid id)
    {
        return await collection.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsAsync()
    {
        return await collection.Find(_ => true).ToListAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(DateTime until)
    {
        return await collection.Find(e => e.Timestamp <= until).ToListAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(DateTime from)
    {
        return await collection.Find(e => e.Timestamp >= from).ToListAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(
        DateTime from,
        DateTime until
    )
    {
        return await collection
            .Find(e => e.Timestamp >= from && e.Timestamp <= until)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdAsync(Guid entityId)
    {
        return await collection.Find(e => e.EntityId == entityId).ToListAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdUntilAsync(
        Guid entityId,
        DateTime until
    )
    {
        return await collection
            .Find(e => e.EntityId == entityId && e.Timestamp <= until)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsByEntityIdFromUntilAsync(
        Guid entityId,
        DateTime from,
        DateTime until
    )
    {
        return await collection
            .Find(e => e.EntityId == entityId && e.Timestamp >= from && e.Timestamp <= until)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromAsync(long fromEventNumber)
    {
        return await collection
            .Find(e => e.EventNumber >= fromEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsUntilAsync(long untilEventNumber)
    {
        return await collection
            .Find(e => e.EventNumber <= untilEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<IEvent>> GetEventsFromUntilAsync(
        long fromEventNumber,
        long untilEventNumber
    )
    {
        return await collection
            .Find(e => e.EventNumber >= fromEventNumber && e.EventNumber <= untilEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();
    }

    private Task<bool> EventExistsAsync(EventBase e) =>
        collection.Find(x => x.Id == e.Id).AnyAsync();
}
