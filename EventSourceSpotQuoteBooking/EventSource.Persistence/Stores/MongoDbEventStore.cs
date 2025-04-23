using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Entities;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class MongoDbEventStore : IEventStore
{
    private readonly IMongoCollection<MongoDbEvent> collection;
    private readonly IPersonalDataInterceptor personalDataInterceptor;
    private readonly IEventSequenceGenerator sequenceGenerator;

    public MongoDbEventStore(
        IMongoDbService mongoDbService,
        IPersonalDataInterceptor personalDataInterceptor,
        IEventSequenceGenerator sequenceGenerator
    )
    {
        this.personalDataInterceptor = personalDataInterceptor;
        this.sequenceGenerator = sequenceGenerator;
        collection = mongoDbService.EventCollection;
    }

    public async Task InsertEventAsync(Event e)
    {
        if (await EventExistsAsync(e))
            return;

        e.EventNumber = await sequenceGenerator.GetNextSequenceNumberAsync();
        e = await personalDataInterceptor.ProcessEventForStorage(e);
        await collection.InsertOneAsync(new MongoDbEvent(e));
    }

    public async Task<Event?> GetEventByIdAsync(Guid id)
    {
        var mongoEvent = await collection.Find(e => e.Id == id).FirstOrDefaultAsync();
        if (mongoEvent is null)
            return null;
        return mongoEvent.ToDomain();
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsAsync()
    {
        var mongoEvents = await collection.Find(_ => true).ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsUntilAsync(DateTime until)
    {
        var mongoEvents = await collection.Find(e => e.Timestamp <= until).ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsFromAsync(DateTime from)
    {
        var mongoEvents = await collection.Find(e => e.Timestamp >= from).ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsFromUntilAsync(
        DateTime from,
        DateTime until
    )
    {
        var mongoEvents = await collection
            .Find(e => e.Timestamp >= from && e.Timestamp <= until)
            .ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsByEntityIdAsync(Guid entityId)
    {
        var mongoEvents = await collection.Find(e => e.EntityId == entityId).ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsByEntityIdUntilAsync(
        Guid entityId,
        DateTime until
    )
    {
        var mongoEvents = await collection
            .Find(e => e.EntityId == entityId && e.Timestamp <= until)
            .ToListAsync();
        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsByEntityIdFromUntilAsync(
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

    public async Task<IReadOnlyCollection<Event>> GetEventsFromAsync(long fromEventNumber)
    {
        var mongoEvents = await collection
            .Find(e => e.EventNumber >= fromEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();

        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsUntilAsync(long untilEventNumber)
    {
        var mongoEvents = await collection
            .Find(e => e.EventNumber <= untilEventNumber)
            .SortBy(e => e.EventNumber)
            .ToListAsync();

        return await ToDomain(mongoEvents);
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsFromUntilAsync(
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

    private async Task<IReadOnlyCollection<Event>> ToDomain(ICollection<MongoDbEvent> mongoDbEvents)
    {
        var domainEvents = mongoDbEvents.Select(e => e.ToDomain()).ToList();

        var processedEvents = await Task.WhenAll(
            domainEvents.Select(e => personalDataInterceptor.ProcessEventForRetrieval(e))
        );

        return processedEvents.ToList();
    }

    private Task<bool> EventExistsAsync(Event e) => collection.Find(x => x.Id == e.Id).AnyAsync();
}
