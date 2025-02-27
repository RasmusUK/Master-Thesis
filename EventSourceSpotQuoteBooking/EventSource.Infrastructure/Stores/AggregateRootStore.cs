using System.Collections.Immutable;
using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Entities;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class AggregateRootStore : IAggregateRootStore
{
    private readonly IMongoCollection<MongoDbAggregateRoot> collection;
    
    public AggregateRootStore(IMongoDbService mongoDbService)
    {
        collection = mongoDbService.AggregateRootCollection;
    }

    public Task SaveAggregateRootAsync(AggregateRoot aggregateRoot)
        => collection.InsertOneAsync(new MongoDbAggregateRoot(aggregateRoot));

    public async Task<IReadOnlyCollection<AggregateRoot>> GetAggregateRootsAsync()
    {
        var mongoAggregateRoots = await collection.Find(_ => true).ToListAsync();
        return mongoAggregateRoots.Select(ar => ar.ToDomain()).ToImmutableList();
    }
}