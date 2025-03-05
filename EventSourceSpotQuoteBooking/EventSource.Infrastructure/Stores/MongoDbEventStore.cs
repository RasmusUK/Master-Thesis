using System.Collections.Immutable;
using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Entities;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class MongoDbEventStore : IEventStore
{
    private readonly IMongoCollection<MongoDbEvent> collection;

    public MongoDbEventStore(IMongoDbService mongoDbService)
    {
        collection = mongoDbService.EventCollection;
    }

    public Task SaveEventAsync(Event e) => collection.InsertOneAsync(new MongoDbEvent(e));

    public async Task<IReadOnlyCollection<Event>> GetEventsAsync()
    {
        var mongoEvents = await collection.Find(_ => true).ToListAsync();
        return mongoEvents.Select(e => e.ToDomain()).ToImmutableList();
    }

    public async Task<IReadOnlyCollection<Event>> GetEventsAsync(Guid aggregateId)
    {
        var mongoEvents = await collection.Find(e => e.EventId == aggregateId).ToListAsync();
        return mongoEvents.Select(e => e.ToDomain()).ToImmutableList();
    }
}
