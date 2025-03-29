using EventSource.Application.Interfaces;
using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Entities;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class MongoDbEventStore : IEventStore, IMongoDbEventStore
{
    private readonly IMongoCollection<MongoDbEvent> collection;
    private readonly IPersonalDataInterceptor personalDataInterceptor;

    public MongoDbEventStore(
        IMongoDbService mongoDbService,
        IPersonalDataInterceptor personalDataInterceptor
    )
    {
        this.personalDataInterceptor = personalDataInterceptor;
        collection = mongoDbService.EventCollection;
    }

    public async Task InsertEventAsync(Event e)
    {
        if (await EventExistsAsync(e))
            return;
        e = await personalDataInterceptor.ProcessEventForStorage(e);
        await collection.InsertOneAsync(new MongoDbEvent(e));
    }

    public async Task InsertEventAsync(Event e, IClientSessionHandle session)
    {
        if (await EventExistsAsync(e))
            return;
        e = await personalDataInterceptor.ProcessEventForStorage(e);
        await collection.InsertOneAsync(session, new MongoDbEvent(e));
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
