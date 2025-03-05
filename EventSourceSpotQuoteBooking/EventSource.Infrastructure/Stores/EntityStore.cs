using System.Collections.Immutable;
using EventSource.Core;
using EventSource.Core.Interfaces;
using EventSource.Persistence.Entities;
using EventSource.Persistence.Interfaces;
using MongoDB.Driver;

namespace EventSource.Persistence.Stores;

public class EntityStore : IEntityStore
{
    private readonly IMongoCollection<MongoDbEntity> collection;

    public EntityStore(IMongoDbService mongoDbService)
    {
        collection = mongoDbService.AggregateRootCollection;
    }

    public Task SaveEntityAsync(Entity entity) =>
        collection.InsertOneAsync(new MongoDbEntity(entity));

    public async Task<T?> GetEntityAsync<T>(Guid id)
        where T : Entity
    {
        var mongoAggregateRoot = await collection.Find(ar => ar.Id == id).FirstOrDefaultAsync();
        return mongoAggregateRoot?.Deserialize<T>();
    }

    public async Task<IReadOnlyCollection<Entity>> GetEntitiesAsync()
    {
        var mongoAggregateRoots = await collection.Find(_ => true).ToListAsync();
        return mongoAggregateRoots.Select(ar => ar.ToDomain()).ToImmutableList();
    }
}
